namespace Fiero.Business
{
    public abstract class SingleTargetEffect : Effect
    {
        public readonly Actor Target;
        public SingleTargetEffect(Actor target)
        {
            Target = target;
        }
    }
}
