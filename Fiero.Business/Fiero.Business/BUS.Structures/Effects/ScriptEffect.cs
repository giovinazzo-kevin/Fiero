using Ergo.Lang.Ast;
using System.Collections.Generic;
using System.Linq;
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
        public readonly Script Script;
        public readonly string Description;

        public ScriptEffect(Script script, string description = null)
        {
            Script = script;
            Description = description ?? string.Empty;
        }

        public override EffectName Name => EffectName.Script;
        public override string DisplayName => Script.Info.Name;
        public override string DisplayDescription => Description;

        protected override void OnStarted(GameSystems systems, Entity owner)
        {
            base.OnStarted(systems, owner);
        }

        protected override IEnumerable<Subscription> RouteEvents(GameSystems systems, Entity owner)
        {
            foreach (var sub in Script.ScriptProperties.SubscribedEvents.Contents)
            {
                switch (sub)
                {
                    case Atom { Value: "actor_turn_started" }:
                        yield return systems.Action.ActorTurnStarted.SubscribeHandler(a =>
                        {
                            Script.Solve(new(new Complex(new Atom("actor_turn_started"), new Atom(a.Actor.Id), new Atom(a.TurnId))))
                                .ToList();
                        });
                        break;
                }
            }
        }
    }
}
