using Unconcern.Common;

namespace Fiero.Business
{
    public class NullEffect : Effect
    {
        public override EffectName Name => EffectName.None;
        public override string DisplayName => "$Effect.None.Name$";
        public override string DisplayDescription => "$Effect.None.Desc$";
        protected override IEnumerable<Subscription> RouteEvents(MetaSystem systems, Entity owner)
        {
            yield break;
        }
    }
}
