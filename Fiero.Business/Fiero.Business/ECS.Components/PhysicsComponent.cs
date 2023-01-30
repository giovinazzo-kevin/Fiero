using Fiero.Core;

namespace Fiero.Business
{
    public class PhysicsComponent : EcsComponent
    {
        public FloorId FloorId { get; set; }
        public Coord Position { get; set; }
        public int Roots { get; set; }
        // Can phase through solid objects
        public bool Phasing { get; set; }
        public bool CanMove { get; set; }
        public bool BlocksMovement { get; set; }
        public bool BlocksNpcPathing { get; set; }
        public bool BlocksPlayerPathing { get; set; }
        public bool BlocksLight { get; set; }
    }
}
