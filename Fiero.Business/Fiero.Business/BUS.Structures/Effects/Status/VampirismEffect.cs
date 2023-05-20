using Fiero.Core;
using Fiero.Core.Extensions;
using System;

namespace Fiero.Business
{
    public class VampirismEffect : HitDealtEffect
    {
        public readonly Func<int, int> GetAmount;

        public VampirismEffect(EffectDef source, int amount) : base(source)
        {
            GetAmount = _ => amount;
        }

        public VampirismEffect(EffectDef source, float percentage) : base(source)
        {
            GetAmount = damage => Math.Max(1, Rng.Random.RoundProportional(percentage * damage));
        }

        public override EffectName Name => EffectName.Vampirism;
        public override string DisplayName => "$Effect.Vampirism.Name$";
        public override string DisplayDescription => "$Effect.Vampirism.Desc$";

        protected override void OnApplied(GameSystems systems, Entity owner, Actor source, Actor target, int damage)
        {
            var hp = GetAmount(damage);
            systems.Action.ActorHealed.HandleOrThrow(new(source, source, owner, hp));
        }
    }
}
