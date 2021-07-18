namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly struct FeatureInteractedWithEvent
        {
            public readonly Actor Actor;
            public readonly Feature Feature;
            public FeatureInteractedWithEvent(Actor actor, Feature feature)
                => (Actor, Feature) = (actor, feature);
        }
    }
}
