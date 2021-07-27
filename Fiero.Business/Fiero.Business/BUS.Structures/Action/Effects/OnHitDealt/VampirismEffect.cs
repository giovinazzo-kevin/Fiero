using Fiero.Core;

namespace Fiero.Business
{
    public class VampirismEffect : HitDealtEffect
    {
        public override string Name => "$Spell.Vampirism.Name$";
        public override string Description => "$Spell.Vampirism.Desc$";

        protected override void OnApplied(GameSystems systems, Entity owner, Actor source, Actor target, int damage)
        {
            var roll = Rng.Random.Between(0, damage);
            source.Log?.Write($"You heal for {roll} HP");
            source.Heal(roll);
        }
    }
}
