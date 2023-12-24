using Unconcern.Common;

namespace Fiero.Business
{

    // Heal a fixed amount of HP 
    public class HealEffect : TypedEffect<Actor>
    {
        public override string DisplayName => "$Effect.Heal.Name$";
        public override string DisplayDescription => "$Effect.Heal.Desc$";
        public override EffectName Name => EffectName.Heal;
        public readonly int Amount;

        public HealEffect(Entity source, int amount)
            : base(source)
        {
            Amount = amount;
        }

        protected override void TypedOnStarted(MetaSystem systems, Actor target)
        {
            systems.Get<ActionSystem>().ActorHealed.HandleOrThrow(new(target, target, target, Amount));
            End(systems, target);
        }

        protected override IEnumerable<Subscription> RouteEvents(MetaSystem systems, Entity owner)
        {
            yield break;
        }
    }
}
