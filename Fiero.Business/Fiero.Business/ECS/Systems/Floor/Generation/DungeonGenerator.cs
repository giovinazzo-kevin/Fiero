using Fiero.Core;

namespace Fiero.Business
{
    [TransientDependency]
    public abstract class DungeonGenerator
    {
        public abstract Floor GenerateFloor(FloorId id, FloorBuilder builder);
    }
}
