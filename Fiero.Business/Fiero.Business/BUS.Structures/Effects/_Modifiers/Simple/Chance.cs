using Fiero.Core;
using System.Collections.Generic;
using Unconcern.Common;

namespace Fiero.Business
{
    public class Chance : ModifierEffect
    {
        public readonly float Probability;

        public Chance(EffectDef source, float chance) : base(source)
        {
            Probability = chance;
        }

        public override string Name => $"$Effect.{Source.Name}$";
        public override string Description => $"$Effect.Chance$ ({(int)(Probability * 100)}%)";
        public override EffectName Type => Source.Name;

        protected override void OnStarted(GameSystems systems, Entity owner)
        {
            if(Rng.Random.NextDouble() < Probability) {
                Source.Resolve().Start(systems, owner);
            }
        }

        protected override IEnumerable<Subscription> RouteEvents(GameSystems systems, Entity owner)
        {
            yield break;
        }
    }
}
