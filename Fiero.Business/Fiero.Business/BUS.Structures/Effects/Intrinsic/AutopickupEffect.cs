namespace Fiero.Business
{
    public class AutopickupEffect : IntrinsicEffect
    {
        public override string DisplayName => "$Intrinsic.Autopickup.Name$";
        public override string DisplayDescription => "$Intrinsic.Autopickup.Desc$";
        public override EffectName Name => EffectName.AutoPickup;

        protected override void OnApplied(MetaSystem systems, Entity owner, Actor target)
        {
            Subscriptions.Add(systems.Get<ActionSystem>().ActorMoved.SubscribeHandler(e =>
            {
                if (e.Actor == target)
                {
                    var itemsHere = systems.Get<DungeonSystem>().GetItemsAt(target.FloorId(), target.Position());
                    if (itemsHere.FirstOrDefault() is { } item && !target.Ai.DislikedItems.Any(f => f(item)))
                    {
                        systems.Get<ActionSystem>().ItemPickedUp.Handle(new(target, item));
                    }
                }
            }));
        }

        protected override void OnRemoved(MetaSystem systems, Entity owner, Actor target)
        {

        }
    }
}
