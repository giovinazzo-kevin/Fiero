using Fiero.Core;

namespace Fiero.Business
{
    public class VampirismEffect : OnHitDealtEffect
    {
        protected override void OnApplied(GameSystems systems, Entity owner, Actor source, Actor target, int damage)
        {
            var roll = Rng.Random.Between(0, damage);
            source.Log?.Write($"You heal for {roll} HP");
            source.Heal(roll);
        }
    }
}
