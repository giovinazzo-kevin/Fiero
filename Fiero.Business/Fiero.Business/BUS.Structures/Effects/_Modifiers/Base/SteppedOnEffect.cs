using System.Collections.Generic;
using Unconcern.Common;

namespace Fiero.Business
{
    /// <summary>
    /// Stepped-on effects can be applied to:
    /// - Tiles
    /// - Features
    /// </summary>
    public abstract class SteppedOnEffect : ModifierEffect
    {
        protected SteppedOnEffect(EffectDef source) : base(source)
        {
        }

        protected abstract void OnApplied(GameSystems systems, Entity owner, Actor target);
        protected abstract void OnRemoved(GameSystems systems, Entity owner, Actor target);

        protected override IEnumerable<Subscription> RouteEvents(GameSystems systems, Entity owner)
        {
            if (owner.TryCast<Tile>(out var tile)) {
                yield return systems.Action.ActorMoved.SubscribeHandler(e => {
                    if (tile.FloorId() != e.Actor.FloorId()) {
                        return;
                    }
                    if (e.NewPosition == tile.Position()) {
                        OnApplied(systems, owner, e.Actor);
                    }
                    else if (e.OldPosition == tile.Position()) {
                        OnRemoved(systems, owner, e.Actor);
                    }
                });
                // End the effect when this tile is removed or destroyed for any reason
                yield return systems.Floor.TileChanged.SubscribeHandler(e => {
                    if (e.OldState == tile) {
                        End();
                    }
                });
            }
            if (owner.TryCast<Feature>(out var feature)) {
                yield return systems.Action.ActorMoved.SubscribeHandler(e => {
                    if(feature.FloorId() != e.Actor.FloorId()) {
                        return;
                    }
                    if (e.NewPosition == feature.Position()) {
                        OnApplied(systems, owner, e.Actor);
                    }
                    else if (e.OldPosition == feature.Position()) {
                        OnRemoved(systems, owner, e.Actor);
                    }
                });
                // End the effect when this feature is removed or destroyed for any reason
                yield return systems.Floor.FeatureRemoved.SubscribeHandler(e => {
                    if (e.OldState == feature) {
                        End();
                    }
                });
            }
        }
    }
}
