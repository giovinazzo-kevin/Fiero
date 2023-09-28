using System.Collections.Generic;
using Unconcern.Common;

namespace Fiero.Business
{
    // The target corpse is raised as either a zombie or a skeleton.
    public class RaiseUndeadEffect : TypedEffect<Corpse>
    {
        public readonly UndeadRaisingName Mode;

        public RaiseUndeadEffect(Entity source, UndeadRaisingName mode) : base(source)
        {
            Mode = mode;
        }

        public override string DisplayName => "$Effect.RaiseUndead.Name$";
        public override string DisplayDescription => "$Effect.RaiseUndead.Desc$";
        public override EffectName Name => EffectName.RaiseUndead;

        protected override void ApplyOnStarted(GameSystems systems, Corpse target)
        {
            systems.Action.CorpseRaised.HandleOrThrow(new(Source, target, Mode));
        }

        protected override IEnumerable<Subscription> RouteEvents(GameSystems systems, Entity owner)
        {
            yield break;
        }
    }
}
