using Ergo.Interpreter.Libraries;
using Ergo.Lang;
using Ergo.Solver;
using System.Collections.Immutable;
using Unconcern.Common;

namespace Fiero.Business
{

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
        public readonly string Description;
        public readonly Hook EffectStartedHook;
        public readonly Hook EffectEndedHook;

        public ScriptEffect(Script script, string description = null)
        {
            Script = script;
            Description = description ?? string.Empty;
            var module = Script.ScriptProperties.Scope.Module;
            EffectStartedHook = new(new(new("began"), Maybe.Some(1), module, default));
            EffectEndedHook = new(new(new("ended"), Maybe.Some(1), module, default));
        }

        public override EffectName Name => EffectName.Script;
        public override string DisplayName => Script.Info.Name;
        public override string DisplayDescription => Description;

        protected SolverContext CreateContext() => SolverContext.Create(Script.ScriptProperties.Solver, Script.ScriptProperties.Scope.InterpreterScope);

        protected override void OnStarted(GameSystems systems, Entity owner)
        {
            base.OnStarted(systems, owner);
            using var ctx = CreateContext();
            var eventTerm = TermMarshall.ToTerm(new EffectStartedEvent(owner), mode: TermMarshalling.Named);
            if (EffectStartedHook.IsDefined(ctx))
            {
                foreach (var _ in EffectStartedHook.Call(ctx, Script.ScriptProperties.Scope, ImmutableArray.Create(eventTerm)))
                    ;
            }
        }

        protected override void OnEnded(GameSystems systems, Entity owner)
        {
            base.OnEnded(systems, owner);
            using var ctx = CreateContext();
            var eventTerm = TermMarshall.ToTerm(new EffectEndedEvent(owner), mode: TermMarshalling.Named);
            if (EffectEndedHook.IsDefined(ctx))
            {
                foreach (var _ in EffectEndedHook.Call(ctx, Script.ScriptProperties.Scope, ImmutableArray.Create(eventTerm)))
                    ;
            }
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
