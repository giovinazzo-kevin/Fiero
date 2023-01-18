using Fiero.Core;

namespace Fiero.Business
{
    public class PhysicsComponent : EcsComponent
    {
        public FloorId FloorId { get; set; }
        public Coord Position { get; set; }
        public int Roots { get; set; }
        public bool CanMove { get; set; }
        public bool BlocksMovement { get; set; }
        public bool BlocksPathing { get; set; }
        public bool BlocksPathingOrMovement => BlocksPathing || BlocksMovement;
        public bool BlocksLight { get; set; }
    }
}
