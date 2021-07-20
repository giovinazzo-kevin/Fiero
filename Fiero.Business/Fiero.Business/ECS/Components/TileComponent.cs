using Fiero.Core;
using System.Drawing;

namespace Fiero.Business
{
    public class TileComponent : EcsComponent
    {
        public TileName Name { get; set; }
        public bool BlocksMovement { get; set; }
        public bool BlocksLight { get; set; }
    }
}
