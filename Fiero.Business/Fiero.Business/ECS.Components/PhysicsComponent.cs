using Fiero.Core;
using System.Drawing;
using System.Numerics;

namespace Fiero.Business
{
    public class PhysicsComponent : EcsComponent
    {
        public FloorId FloorId { get; set; }
        public Coord Position { get; set; }
        public bool CanMove { get; set; }
        public bool BlocksMovement { get; set; }
        public bool BlocksLight { get; set; }
    }
}
