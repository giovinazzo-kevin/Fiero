using Fiero.Core;

namespace Fiero.Business
{
    [TransientDependency]
    public abstract class BranchGenerator
    {
        public abstract Floor GenerateFloor(FloorId id, FloorBuilder builder);
    }
}
