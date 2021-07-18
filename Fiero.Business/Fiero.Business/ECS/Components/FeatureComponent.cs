using Fiero.Core;
using System.Collections;
using System.Linq;

namespace Fiero.Business
{
    public class FeatureComponent : EcsComponent
    {
        public FloorId FloorId { get; set; }
        public FeatureName Type { get; set; }
        public bool BlocksMovement { get; set; }
    }
}
