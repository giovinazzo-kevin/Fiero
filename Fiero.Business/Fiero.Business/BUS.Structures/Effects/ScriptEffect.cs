using Ergo.Interpreter;
using Ergo.Interpreter.Directives;
using Ergo.Interpreter.Libraries;
using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Solver;
using Ergo.Solver.BuiltIns;
using System.Collections.Immutable;
using Unconcern.Common;

namespace Fiero.Business
{

    internal class EndEffect : SolverBuiltIn
    {
        protected readonly ScriptEffect _source;
        protected readonly Entity _owner;
        protected readonly GameSystems _systems;
        private bool _ended = false;

        public EndEffect(GameSystems systems, ScriptEffect source, Entity owner, Atom module) : base(string.Empty, new Atom("end"), 0, module)
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

    internal class ScriptEffectLib : Library
    {
        private readonly Atom _module;
        public override Atom Module => _module;
        protected readonly ScriptEffect _source;
        protected readonly GameSystems _systems;
        protected readonly Entity _owner;
        public ScriptEffectLib(GameSystems systems, ScriptEffect source, Entity owner, Atom module)
        {
            _module = module;
            _source = source;
            _owner = owner;
            _systems = systems;
        }
        public override IEnumerable<SolverBuiltIn> GetExportedBuiltins()
        {
            yield return new EndEffect(_systems, _source, _owner, _module);
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
        public readonly record struct EffectStartedEvent(Entity Owner);
        public readonly record struct EffectEndedEvent(Entity Owner);

        public readonly Script Script;
        public readonly InterpreterScope Scope;
        public readonly string Description;
        public readonly Hook EffectStartedHook;
        public readonly Hook EffectEndedHook;

        public readonly Dictionary<int, SolverContext> Contexts = new();

        public ScriptEffect(Script script, string description = null)
        {
            Script = script;
            var s = script.ScriptProperties.Scope.InterpreterScope;
            var m = s.Modules[script.ScriptProperties.Scope.Module];
            Description = description ?? string.Empty;
            var module = Script.ScriptProperties.Scope.Module;
            EffectStartedHook = new(new(new("began"), Maybe.Some(1), module, default));
            EffectEndedHook = new(new(new("ended"), Maybe.Some(1), module, default));
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
                .WithLinkedLibrary(new ScriptEffectLib(systems, this, owner, m)));
            return Contexts[owner.Id] = SolverContext.Create(Script.ScriptProperties.Solver, newScope);
        }

        protected override void OnStarted(GameSystems systems, Entity owner)
        {
            base.OnStarted(systems, owner);
            if (Script.ScriptProperties.LastError != null)
                return;
            var ctx = GetOrCreateContext(systems, owner);
            var scope = Script.ScriptProperties.Scope.WithInterpreterScope(ctx.Scope);
            var eventTerm = TermMarshall.ToTerm(new EffectStartedEvent(owner), mode: TermMarshalling.Named);
            if (EffectStartedHook.IsDefined(ctx))
            {
                foreach (var _ in EffectStartedHook.Call(ctx, scope, ImmutableArray.Create(eventTerm)))
                    ;
            }
        }

        protected override void OnEnded(GameSystems systems, Entity owner)
        {
            base.OnEnded(systems, owner);
            var ctx = GetOrCreateContext(systems, owner);
            var scope = Script.ScriptProperties.Scope.WithInterpreterScope(ctx.Scope);
            var eventTerm = TermMarshall.ToTerm(new EffectEndedEvent(owner), mode: TermMarshalling.Named);
            if (EffectEndedHook.IsDefined(ctx))
            {
                foreach (var _ in EffectEndedHook.Call(ctx, scope, ImmutableArray.Create(eventTerm)))
                    ;
            }
            Contexts.Remove(owner.Id);
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
