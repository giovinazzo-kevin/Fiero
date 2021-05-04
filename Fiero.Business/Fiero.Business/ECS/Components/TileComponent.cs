using Fiero.Core;
using System.Drawing;

namespace Fiero.Business
{
    public class TileComponent : Component
    {
        public TileName Name { get; set; }
        public bool BlocksMovement { get; set; }

    }
}
