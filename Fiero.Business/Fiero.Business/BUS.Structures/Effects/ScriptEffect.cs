using Ergo.Interpreter;
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
        internal class End : BuiltIn
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

            public override IEnumerable<Evaluation> Apply(SolverContext solver, SolverScope scope, ImmutableArray<ITerm> arguments)
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
        internal class Owner : BuiltIn
        {
            protected readonly EntityAsTerm _owner;

            public Owner(Entity owner, Atom module) : base(string.Empty, new Atom("owner_"), 1, module)
            {
                _owner = new(owner.Id, owner.ErgoType());
            }

            public override IEnumerable<Evaluation> Apply(SolverContext solver, SolverScope scope, ImmutableArray<ITerm> arguments)
            {
                if (_owner.Unify(arguments[0]).TryGetValue(out var subs))
                {
                    yield return True(subs);
                    yield break;
                }
                yield return False();
                yield break;
            }
        }
        internal class Args : BuiltIn
        {
            protected readonly string _args;

            public Args(string args, Atom module) : base(string.Empty, new Atom("args"), 1, module)
            {
                _args = args;
            }

            public override IEnumerable<Evaluation> Apply(SolverContext solver, SolverScope scope, ImmutableArray<ITerm> arguments)
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
        internal class Subscribed : BuiltIn
        {
            private readonly List<Signature> _subs;

            public Subscribed(List<Signature> subs, Atom module) : base(string.Empty, new Atom("subscribed"), 2, module)
            {
                _subs = subs;
            }

            public override IEnumerable<Evaluation> Apply(SolverContext solver, SolverScope scope, ImmutableArray<ITerm> arguments)
            {
                var any = false;
                foreach (var sign in _subs)
                {
                    if (!arguments[0].Unify(sign.Module.GetOrThrow(new InvalidOperationException())).TryGetValue(out var subs0))
                    {
                        continue;
                    }
                    if (!arguments[1].Unify(sign.Functor).TryGetValue(out var subs1))
                    {
                        yield return False();
                        yield break;
                    }
                    yield return True(SubstitutionMap.MergeRef(subs0, subs1));
                    any = true;
                }
                if (!any)
                {
                    yield return False();
                    yield break;
                }
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
        public override IEnumerable<BuiltIn> GetExportedBuiltins()
        {
            yield return new End(_systems, _source, _owner, _module);
            yield return new Args(_args, _module);
            yield return new Owner(_owner, _module);
            yield return new Subscribed(_source.Script.ScriptProperties.SubscribedEvents, _module);
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
        public Maybe<CompiledHook> EffectStartedHook { get; private set; }
        public Maybe<CompiledHook> EffectEndedHook { get; private set; }
        public Maybe<CompiledHook> ClearDataHook { get; private set; }
        public readonly string ArgumentsString;

        public readonly ConcurrentDictionary<int, SolverContext> Contexts = new();

        public ScriptEffect(Script script, string arguments, string description = null)
        {
            Script = script;
            ArgumentsString = arguments;
            Description = description ?? string.Empty;
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
            var lib = new ScriptEffectLib(systems, this, owner, ArgumentsString, m);
            // Since the kb is already created by now, we need to assert these builtins manually
            foreach (var b in lib.GetExportedBuiltins())
            {
                var pred = new Predicate(b);
                Script.ScriptProperties.Solver.KnowledgeBase.AssertZ(pred);
                Script.ScriptProperties.Solver.KnowledgeBase.DependencyGraph.AddNode(pred);
            }
            var module = Script.ScriptProperties.Scope.Module;
            EffectStartedHook = new Hook(new(new("began"), Maybe.Some(0), module, default))
                .Compile(Script.ScriptProperties.Solver.KnowledgeBase);
            EffectEndedHook = new Hook(new(new("ended"), Maybe.Some(0), module, default))
                .Compile(Script.ScriptProperties.Solver.KnowledgeBase);
            ClearDataHook = new Hook(new(new("clear"), Maybe.Some(0), ScriptingSystem.DataModule, default))
                .Compile(Script.ScriptProperties.Solver.KnowledgeBase);
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
            var args = ImmutableArray.Create<ITerm>();
            if (EffectStartedHook.TryGetValue(out var hook))
            {
                foreach (var _ in hook.Call(ctx, scope, args))
                    ;
            }
        }

        protected override void OnEnded(GameSystems systems, Entity owner)
        {
            base.OnEnded(systems, owner);
            var ctx = GetOrCreateContext(systems, owner);
            var scope = Script.ScriptProperties.Scope
                .WithInterpreterScope(ctx.Scope);
            var args = ImmutableArray.Create<ITerm>();
            if (EffectEndedHook.TryGetValue(out var hook))
            {
                foreach (var _ in hook.Call(ctx, scope, args))
                    ;
            }
            if (ClearDataHook.TryGetValue(out hook))
            {
                foreach (var _ in hook.Call(ctx, scope, args))
                    ;
            }
            Contexts.Remove(owner.Id, out _);
            ctx.Dispose();
        }

        protected override IEnumerable<Subscription> RouteEvents(GameSystems systems, Entity owner)
        {
            _ = GetOrCreateContext(systems, owner);
            /* Ergo scripts can subscribe to Fiero events via the subscribe/2 directive.
               All the directive does is prepare a list for this method, which is
               called whenever an effect that is tied to a script resolves. The list
               contains the signatures of the events that the script is handling.

               By wiring each event to a call to the script's solver, we can interpret
               the result of that call as the EventResult to pass to the owning system.

                Additionally, Ergo scripts may raise and handle events of their own.
            */
            var routes = ScriptingSystem.GetScriptRoutes();
            var visibleScripts = systems.Scripting.GetVisibleScripts();
            var subbedEvents = Script.ScriptProperties.SubscribedEvents;
            foreach (var sig in subbedEvents)
            {
                if (routes.TryGetValue(sig, out var sub))
                {
                    yield return sub(this, systems);
                }
                // Could be from a script, in which case it's not unknown so much as it's dynamic
                else if (!visibleScripts.Contains(sig.Module.GetOrThrow(new InvalidOperationException()).Explain()))
                {
                    Script.ScriptProperties.Solver.Out
                        .WriteLine($"WRN: Unknown event route: {sig.Explain()}");
                    Script.ScriptProperties.Solver.Out.Flush();
                }
            }
        }
    }
}
