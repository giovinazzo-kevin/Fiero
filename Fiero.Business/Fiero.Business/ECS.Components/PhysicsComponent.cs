namespace Fiero.Business
{
    public class PhysicsComponent : EcsComponent
    {
        public FloorId FloorId { get; set; }
        public Coord Position { get; set; }
        public int Roots { get; set; }
        // Can phase through solid objects
        public bool Phasing { get; set; }
        // Can fly above the ground
        public bool Flying { get; set; }
        public bool CanMove { get; set; }
        public bool IsFlat { get; set; }
        public bool SwallowsItems { get; set; }
        public bool BlocksMovement { get; set; }
        public bool BlocksNpcPathing { get; set; }
        public bool BlocksPlayerPathing { get; set; }
        public bool BlocksLight { get; set; }
        public int MoveDelay { get; set; } = 100; // Ticks per unit
    }
}
