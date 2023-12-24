using Unconcern.Common;

namespace Fiero.Business
{
    public sealed class ImpassibleEffect : TypedEffect<Actor>
    {
        public ImpassibleEffect(Entity source) : base(source)
        {
        }

        public override EffectName Name => EffectName.Impassible;
        public override string DisplayName => "$Effect.Impassible.Name$";
        public override string DisplayDescription => "$Effect.Impassible.Desc$";
        protected override void TypedOnStarted(MetaSystem systems, Actor target)
        {
            if (target.Effects != null)
                target.Effects.Lock = true;
        }
        protected override void TypedOnEnded(MetaSystem systems, Actor target)
        {
            if (target.Effects != null)
                target.Effects.Lock = false;
        }
        protected override IEnumerable<Subscription> RouteEvents(MetaSystem systems, Entity owner)
        {
            yield break;
        }
    }
}
