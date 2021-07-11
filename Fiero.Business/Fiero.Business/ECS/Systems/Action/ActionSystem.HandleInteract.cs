using Fiero.Core;
using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Unconcern.Common;

namespace Fiero.Business
{
    public partial class ActionSystem : EcsSystem
    {
        protected virtual bool HandleInteract(Actor actor, ref IAction action, ref int? cost)
        {
            if (action is InteractWithFeatureAction useFeature)
                return HandleUseFeature(useFeature.Feature);
            if (action is InteractRelativeAction genericUse)
                return HandleGenericUse(genericUse.Coord, ref action, ref cost);
            throw new NotSupportedException();

            bool HandleGenericUse(Coord point, ref IAction action, ref int? cost)
            {
                // Use handles both grabbing items from the ground and using dungeon features
                var usePos = actor.Physics.Position + point;
                var itemsHere = _floorSystem.ItemsAt(usePos);
                var featuresHere = _floorSystem.FeaturesAt(usePos);
                if (itemsHere.Any() && actor.Inventory != null) {
                    var item = itemsHere.Single();
                    action = new PickUpItemAction(item);
                    return HandlePickUpItem(item);
                }
                else if (featuresHere.Any()) {
                    var feature = featuresHere.Single();
                    action = new InteractWithFeatureAction(feature);
                    return HandleUseFeature(feature);
                }
                return true;
            }

            bool HandleUseFeature(Feature feature)
            {
                if (feature.Properties.Type == FeatureName.Shrine) {
                    actor.Log?.Write($"$Action.YouKneelAt$ {feature.Info.Name}.");
                }
                if (feature.Properties.Type == FeatureName.Chest) {
                    actor.Log?.Write($"$Action.YouOpenThe$ {feature.Info.Name}.");
                }
                return true;
            }

            bool HandlePickUpItem(Item item)
            {
                if (actor.Inventory.TryPut(item)) {
                    _floorSystem.CurrentFloor.RemoveItem(item.Id);
                    actor.Log?.Write($"$Action.YouPickUpA$ {item.DisplayName}.");
                }
                else {
                    actor.Log?.Write($"$Action.YourInventoryIsTooFullFor$ {item.DisplayName}.");
                }
                return true;
            }
        }
    }
}
