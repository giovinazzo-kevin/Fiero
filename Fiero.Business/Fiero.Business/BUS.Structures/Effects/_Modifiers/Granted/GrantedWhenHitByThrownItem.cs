using Fiero.Core;

namespace Fiero.Business
{
    public class GrantedWhenHitByThrownItem : ThrowEffect
    {
        public GrantedWhenHitByThrownItem(EffectDef source) : base(source)
        {
        }

        public override string DisplayName => $"$Effect.{Source.Name}$";
        public override string DisplayDescription => "$Effect.GrantedWhenHitByThrownItem$";
        public override EffectName Name => Source.Name;

        protected override void OnApplied(GameSystems systems, Entity owner, Actor source, Actor target)
        {
            Source.Resolve(source).Start(systems, target);
        }

        protected override void OnApplied(GameSystems systems, Entity owner, Actor source, Coord location)
        {

        }
    }
}
