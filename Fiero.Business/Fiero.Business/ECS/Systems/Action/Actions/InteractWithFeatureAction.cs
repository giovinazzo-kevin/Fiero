namespace Fiero.Business
{
    public readonly struct InteractWithFeatureAction : IAction
    {
        public readonly Feature Feature;
        public InteractWithFeatureAction(Feature feature)
        {
            Feature = feature;
        }
        ActionName IAction.Name => ActionName.Interact;
        int? IAction.Cost => 1;
    }
}
