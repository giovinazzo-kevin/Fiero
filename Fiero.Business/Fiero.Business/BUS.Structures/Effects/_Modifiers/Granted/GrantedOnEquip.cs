namespace Fiero.Business
{
    public class GrantedOnEquip : EquipmentEffect
    {
        protected Effect Instance { get; private set; }

        public GrantedOnEquip(EffectDef source) : base(source)
        {
        }

        public override EffectName Name => Source.Name;
        public override string DisplayName => $"$Effect.{Source.Name}$";
        public override string DisplayDescription => "$Effect.GrantedOnEquip$";
        protected override void OnApplied(GameSystems systems, Actor target)
        {
            (Instance = Source.Resolve(target)).Start(systems, target);
        }
        protected override void OnRemoved(GameSystems systems, Actor target)
        {
            if (Instance != null)
                Instance.End(systems, target);
            Instance = null;
        }
    }
}
