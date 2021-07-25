using Fiero.Core;

namespace Fiero.Business
{
    public class Feature : Drawable
    {
        [RequiredComponent]
        public FeatureComponent FeatureProperties { get; private set; }
        public DialogueComponent Dialogue { get; private set; }
        public PortalComponent Portal { get; private set; }
    }
}
