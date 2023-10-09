﻿using Ergo.Interpreter;
using Ergo.Interpreter.Directives;
using Ergo.Interpreter.Libraries;
using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using Ergo.Solver;
using Ergo.Solver.BuiltIns;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using Unconcern.Common;

namespace Fiero.Business
{
    internal class ScriptEffectLib : Library
    {
        internal class End : SolverBuiltIn
        {
            protected readonly ScriptEffect _source;
            protected readonly Entity _owner;
            protected readonly GameSystems _systems;
            private bool _ended = false;

            public End(GameSystems systems, ScriptEffect source, Entity owner, Atom module) : base(string.Empty, new Atom("end"), 0, module)
            {
                _source = source;
                _owner = owner;
                _systems = systems;
            }

            public override IEnumerable<Evaluation> Apply(SolverContext solver, SolverScope scope, ITerm[] arguments)
            {
                if (_ended)
                {
                    yield return False();
                    yield break;
                }
                _source.End(_systems, _owner);
                _ended = true;
                yield return True();
            }
        }
        internal class Owner : SolverBuiltIn
        {
            protected readonly Entity _owner;

            public Owner(Entity owner, Atom module) : base(string.Empty, new Atom("owner"), 1, module)
            {
                _owner = owner;
            }

            public override IEnumerable<Evaluation> Apply(SolverContext solver, SolverScope scope, ITerm[] arguments)
            {
                var term = TermMarshall.ToTerm(_owner);
                if (arguments[0].Unify(term).TryGetValue(out var subs))
                {
                    yield return True(subs);
                    yield break;
                }
                else if (arguments[0].IsAbstract<Dict>().TryGetValue(out var d1)
                      && term.IsAbstract<Dict>().TryGetValue(out var d2)
                      && d1.Dictionary.TryGetValue(new("id"), out var id1)
                      && d2.Dictionary.TryGetValue(new("id"), out var id2)
                      && id1.Equals(id2))
                {
                    yield return True();
                    yield break;
                }
                yield return False();
                yield break;
            }
        }
        internal class Args : SolverBuiltIn
        {
            protected readonly string _args;

            public Args(string args, Atom module) : base(string.Empty, new Atom("args"), 1, module)
            {
                _args = args;
            }

            public override IEnumerable<Evaluation> Apply(SolverContext solver, SolverScope scope, ITerm[] arguments)
            {
                if (!solver.Solver.Facade.Parse<ITerm>(scope.InterpreterScope, _args)
                    .TryGetValue(out var term))
                {
                    yield return False();
                    yield break;
                }
                if (arguments[0].Unify(term).TryGetValue(out var subs))
                {
                    yield return True(subs);
                    yield break;
                }
                yield return False();
                yield break;
            }
        }


        private readonly Atom _module;
        public override Atom Module => _module;
        protected readonly ScriptEffect _source;
        protected readonly GameSystems _systems;
        protected readonly Entity _owner;
        protected readonly string _args;
        public ScriptEffectLib(GameSystems systems, ScriptEffect source, Entity owner, string arguments, Atom module)
        {
            _module = module;
            _source = source;
            _owner = owner;
            _systems = systems;
            _args = arguments;
        }
        public override IEnumerable<SolverBuiltIn> GetExportedBuiltins()
        {
            yield return new End(_systems, _source, _owner, _module);
            yield return new Args(_args, _module);
            yield return new Owner(_owner, _module);
        }
        public override IEnumerable<InterpreterDirective> GetExportedDirectives()
        {
            yield break;
        }
    }

    /// <summary>
    /// Script effects can be applied to:
    /// - Entities:
    ///     - The effect is applied to the entity when the effect starts, and is removed when the effect ends.
    /// </summary>
    public class ScriptEffect : Effect
    {
        public readonly Script Script;
        public readonly InterpreterScope Scope;
        public readonly string Description;
        public readonly Hook EffectStartedHook;
        public readonly Hook EffectEndedHook;
        public readonly string ArgumentsString;

        public readonly ConcurrentDictionary<int, SolverContext> Contexts = new();

        public ScriptEffect(Script script, string arguments, string description = null)
        {
            Script = script;
            ArgumentsString = arguments;
            var s = script.ScriptProperties.Scope.InterpreterScope;
            var m = s.Modules[script.ScriptProperties.Scope.Module];
            Description = description ?? string.Empty;
            var module = Script.ScriptProperties.Scope.Module;
            EffectStartedHook = new(new(new("began"), Maybe.Some(0), module, default));
            EffectEndedHook = new(new(new("ended"), Maybe.Some(0), module, default));
        }

        public override EffectName Name => EffectName.Script;
        public override string DisplayName => Script.Info.Name;
        public override string DisplayDescription => Description;

        protected SolverContext GetOrCreateContext(GameSystems systems, Entity owner)
        {
            if (Contexts.TryGetValue(owner.Id, out var ctx))
                return ctx;
            var m = Script.ScriptProperties.Scope.Module;
            var newScope = Script.ScriptProperties.Scope.InterpreterScope;
            newScope = newScope.WithModule(newScope.Modules[m]
                .WithLinkedLibrary(new ScriptEffectLib(systems, this, owner, ArgumentsString, m)));
            return Contexts[owner.Id] = SolverContext.Create(Script.ScriptProperties.Solver, newScope);
        }

        protected override void OnStarted(GameSystems systems, Entity owner)
        {
            base.OnStarted(systems, owner);
            if (Script.ScriptProperties.LastError != null)
                return;
            var ctx = GetOrCreateContext(systems, owner);
            var scope = Script.ScriptProperties.Scope
                .WithInterpreterScope(ctx.Scope);
            if (EffectStartedHook.IsDefined(ctx))
            {
                foreach (var _ in EffectStartedHook.Call(ctx, scope, ImmutableArray.Create<ITerm>()))
                    ;
            }
        }

        protected override void OnEnded(GameSystems systems, Entity owner)
        {
            base.OnEnded(systems, owner);
            var ctx = GetOrCreateContext(systems, owner);
            var scope = Script.ScriptProperties.Scope
                .WithInterpreterScope(ctx.Scope);
            if (EffectEndedHook.IsDefined(ctx))
            {
                foreach (var _ in EffectEndedHook.Call(ctx, scope, ImmutableArray.Create<ITerm>()))
                    ;
            }
            Contexts.Remove(owner.Id, out _);
            ctx.Dispose();
        }

        protected override IEnumerable<Subscription> RouteEvents(GameSystems systems, Entity owner)
        {
            /* Ergo scripts can subscribe to Fiero events via the subscribe/2 directive.
               All the directive does is prepare a list for this method, which is
               called whenever an effect that is tied to a script resolves. The list
               contains the signatures of the events that the script is handling.

               By wiring each event to a call to the script's solver, we can interpret
               the result of that call as the EventResult to pass to the owning system.
            */
            var routes = ErgoScriptingSystem.GetScriptRoutes();
            var subbedEvents = Script.ScriptProperties.SubscribedEvents;
            foreach (var sig in subbedEvents)
            {
                if (routes.TryGetValue(sig, out var sub))
                {
                    yield return sub(this, systems);
                }
                else
                {
                    Script.ScriptProperties.Solver.Out
                        .WriteLine($"ERR: Unknown route: {sig.Explain()}");
                    Script.ScriptProperties.Solver.Out.Flush();
                }
            }
        }
    }
}
