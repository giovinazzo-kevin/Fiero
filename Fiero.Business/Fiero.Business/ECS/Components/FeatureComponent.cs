using Fiero.Core;
using System.Collections;
using System.Linq;

namespace Fiero.Business
{
    public class FeatureComponent : Component
    {
        public FeatureName Type { get; set; }
        public bool BlocksMovement { get; set; }
    }
}
