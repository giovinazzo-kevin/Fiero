using Fiero.Core;

namespace Fiero.Business
{
    public class Feature : PhysicalEntity
    {
        [RequiredComponent]
        public FeatureComponent FeatureProperties { get; private set; }
        public DialogueComponent Dialogue { get; private set; }
    }
}
