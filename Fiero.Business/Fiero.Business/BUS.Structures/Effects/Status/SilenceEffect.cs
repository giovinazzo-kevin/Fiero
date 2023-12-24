using Unconcern.Common;

namespace Fiero.Business
{
    // Unable to cast spells and read scrolls, but can still zap wands. 
    public class SilenceEffect : TypedEffect<Actor>
    {
        public SilenceEffect(Entity source) : base(source) { }
        public override string DisplayName => "$Effect.Silence.Name$";
        public override string DisplayDescription => "$Effect.Silence.Desc$";
        public override EffectName Name => EffectName.Silence;

        protected override void TypedOnStarted(MetaSystem systems, Actor target) { }

        protected override IEnumerable<Subscription> RouteEvents(MetaSystem systems, Entity owner)
        {
            yield return systems.Get<ActionSystem>().ActorIntentSelected.SubscribeResponse(e =>
            {
                if (e.Actor == owner && Rng.Random.NChancesIn(2, 3))
                {
                    var dir = new Coord(Rng.Random.Between(-1, 1), Rng.Random.Between(-1, 1));
                    return e.Intent.Name switch
                    {
                        ActionName.Cast => new(new FailAction()) /* TODO: Log message that you can't cast */,
                        ActionName.Read => new(new FailAction()) /* TODO: Log message that you can't read */,
                        _ => new(e.Intent)
                    };
                }
                return new();
            });
        }
    }
}
