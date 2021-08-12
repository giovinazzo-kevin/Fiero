using System.Linq;

namespace Fiero.Business
{
    public class AutopickupEffect : IntrinsicEffect
    {
        public override string Name => "$Intrinsic.Autopickup.Name$";
        public override string Description => "$Intrinsic.Autopickup.Desc$";
        public override EffectName Type => EffectName.Autopickup;

        protected override void OnApplied(GameSystems systems, Entity owner, Actor target)
        {
            Subscriptions.Add(systems.Action.ActorMoved.SubscribeHandler(e => {
                if(e.Actor == target) {
                    var itemsHere = systems.Floor.GetItemsAt(target.FloorId(), target.Position());
                    if(itemsHere.FirstOrDefault() is { } item) {
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
