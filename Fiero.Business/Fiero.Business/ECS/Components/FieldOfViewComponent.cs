using Fiero.Core;
using System.Collections.Generic;

namespace Fiero.Business
{
    public class FieldOfViewComponent : EcsComponent
    {
        public int Radius { get; set; }
        public HashSet<Coord> KnownTiles { get; private set; } = new();
        public HashSet<Coord> VisibleTiles { get; private set; } = new();
    }
}
