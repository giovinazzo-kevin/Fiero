namespace Fiero.Business
{
    public abstract class Effect
    {
        public abstract void OnEffectStarted();
        public abstract void OnEffectEnded();
        public abstract void OnTurnStarted();
        public abstract void OnTurnEnded();
    }

    public abstract class PassiveEffect : SingleTargetEffect
    {
        protected PassiveEffect(Actor target) 
            : base(target)
        {
        }

        public override void OnTurnEnded() { }
        public override void OnTurnStarted() { }
    }
}
