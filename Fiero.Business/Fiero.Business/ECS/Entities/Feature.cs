using Fiero.Core;

namespace Fiero.Business
{
    public class Feature : Drawable
    {
        [RequiredComponent]
        public FeatureComponent Properties { get; private set; }
        public DialogueComponent Dialogue { get; private set; }
    }
}
