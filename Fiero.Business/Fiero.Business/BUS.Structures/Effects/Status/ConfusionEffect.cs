using Fiero.Core;

using System.Collections.Generic;
using Unconcern.Common;

namespace Fiero.Business
{
    // High chance to change the direction of your attack, throw, zap or move into a random direction; can't read scrolls.
    public class ConfusionEffect : TypedEffect<Actor>
    {
        public ConfusionEffect(Entity source) : base(source) { }
        public override string DisplayName => "$Effect.Confusion.Name$";
        public override string DisplayDescription => "$Effect.Confusion.Desc$";
        public override EffectName Name => EffectName.Confusion;

        protected override void TypedOnStarted(GameSystems systems, Actor target) { }

        protected override IEnumerable<Subscription> RouteEvents(GameSystems systems, Entity owner)
        {
            yield return systems.Action.ActorIntentSelected.SubscribeResponse(e =>
            {
                if (e.Actor == owner && Rng.Random.NChancesIn(2, 3))
                {
                    var dir = new Coord(Rng.Random.Between(-1, 1), Rng.Random.Between(-1, 1));
                    return e.Intent.Name switch
                    {
                        ActionName.Move => new(new MoveRelativeAction(dir)),
                        ActionName.MeleeAttack => new(new MoveRelativeAction(dir)),
                        ActionName.Throw when e.Intent is ThrowItemAtPointAction x => new(new ThrowItemAtPointAction(dir, x.Item)),
                        ActionName.Throw when e.Intent is ThrowItemAtOtherAction x => new(new ThrowItemAtPointAction(dir, x.Item)),
                        ActionName.Read => new(new FailAction()) /* TODO: Log message that you can't read */,
                        _ => new(e.Intent)
                    };
                }
                return new();
            });
        }
    }
}
