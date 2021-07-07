using Fiero.Core;
using System.Drawing;

namespace Fiero.Bio
{
    public static partial class Data
    {
        public static class UI
        {
            public static readonly GameDatum<int> TileSize = new(nameof(UI) + nameof(TileSize));
            public static readonly GameDatum<Coord> WindowSize = new(nameof(UI) + nameof(WindowSize));
        }

    }
}
