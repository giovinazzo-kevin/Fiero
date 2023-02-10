using System.Linq;

namespace Fiero.Business
{
    public class AutopickupEffect : IntrinsicEffect
    {
        public override string DisplayName => "$Intrinsic.Autopickup.Name$";
        public override string DisplayDescription => "$Intrinsic.Autopickup.Desc$";
        public override EffectName Name => EffectName.Autopickup;

        protected override void OnApplied(GameSystems systems, Entity owner, Actor target)
        {
            Subscriptions.Add(systems.Action.ActorMoved.SubscribeHandler(e =>
            {
                if (e.Actor == target)
                {
                    var itemsHere = systems.Dungeon.GetItemsAt(target.FloorId(), target.Position());
                    if (itemsHere.FirstOrDefault() is { } item && !target.Ai.DislikedItems.Any(f => f(item)))
                    {
                        systems.Action.ItemPickedUp.HandleOrThrow(new(target, item));
                    }
                }
            }));
        }

        protected override void OnRemoved(GameSystems systems, Entity owner, Actor target)
        {

        }
    }
}
