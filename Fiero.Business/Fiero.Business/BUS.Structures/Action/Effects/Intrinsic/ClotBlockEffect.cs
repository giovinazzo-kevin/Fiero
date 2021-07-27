using Fiero.Core;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Fiero.Business
{
    /// <summary>
    /// Enemies in a small area around you have a chance of being immobilized for one turn when stepping on blood puddles
    /// </summary>
    public class ClotBlockEffect : IntrinsicEffect
    {
        public override string Name => "$Spell.ClotBlock.Name$";
        public override string Description => "$Spell.ClotBlock.Desc$";

        protected override void OnApplied(GameSystems systems, Entity owner, Actor target)
        {
            var squaredArea = 5 * 5;
            Subscriptions.Add(systems.Action.ActorMoved.SubscribeHandler(e => {
                if (!target.IsHostileTowards(e.Actor) || target.SquaredDistanceFrom(e.Actor) > squaredArea)
                    return;
                var splatterHere = systems.Floor.GetFeaturesAt(e.Actor.FloorId(), e.Actor.Position())
                    .TrySelect(f => (f.TryCast<BloodSplatter>(out var splatter), splatter))
                    .SingleOrDefault();
                if (splatterHere is null)
                    return;
                if (Rng.Random.NChancesIn(splatterHere.Blood.Amount / 4f, splatterHere.Blood.MaximumAmount / 2f)) {
                    e.Actor.Physics.Position = e.OldPosition;
                    target.Log?.Write($"$Spell.ClotBlock.ProcMessage$ {e.Actor.Info.Name}");
                }
            }));
        }

        protected override void OnRemoved(GameSystems systems, Entity owner, Actor target)
        {

        }
    }
}
