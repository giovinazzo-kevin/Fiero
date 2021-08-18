using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unconcern.Common;

namespace Fiero.Business
{
    public class Temporary : ModifierEffect
    {
        public readonly int Duration;
        public int Time { get; private set; }
        public override string DisplayName => $"$Effect.{Source.Name}$";
        public override string DisplayDescription => $"$Effect.Temporary$ ({Time})";
        public override EffectName Name => Source.Name;

        public Temporary(EffectDef source, int duration) : base(source)
        {
            Duration = duration;
        }

        protected override void OnStarted(GameSystems systems, Entity owner)
        {
            base.OnStarted(systems, owner);
            var effect = Source.Resolve(null);
            Ended += Temporary_Ended;
            effect.Start(systems, owner);

            void Temporary_Ended(Effect _)
            {
                effect.End();
                Ended -= Temporary_Ended;
            }
        }

        protected override IEnumerable<Subscription> RouteEvents(GameSystems systems, Entity owner)
        {
            yield return systems.Action.TurnEnded.SubscribeHandler(e => {
                if (Time++ >= Duration) {
                    End();
                }
            });
        }
    }
}
