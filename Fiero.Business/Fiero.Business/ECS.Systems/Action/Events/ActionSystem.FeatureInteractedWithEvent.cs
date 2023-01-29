namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly record struct FeatureInteractedWithEvent(Actor Actor, Feature Feature);
    }
}
