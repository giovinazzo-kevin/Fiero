using Ergo.Lang;
using Fiero.Core;
using System.Collections.Generic;

namespace Fiero.Business
{
    public class FieldOfViewComponent : EcsComponent
    {
        public int Radius { get; set; }
        public VisibilityName Sight { get; set; } = VisibilityName.Visible;
        [NonTerm]
        public Dictionary<FloorId, HashSet<Coord>> KnownTiles { get; private set; } = new();
        [NonTerm]
        public Dictionary<FloorId, HashSet<Coord>> VisibleTiles { get; private set; } = new();
    }
}
