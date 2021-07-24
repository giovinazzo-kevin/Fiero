using Fiero.Core;
using System.Collections;
using System.Linq;

namespace Fiero.Business
{
    public class FeatureComponent : EcsComponent
    {
        public FloorId FloorId { get; set; }
        public FeatureName Name { get; set; }
        public bool BlocksMovement { get; set; }
        public bool BlocksLight { get; set; }
    }
}
