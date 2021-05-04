using Fiero.Core;
using System.Drawing;

namespace Fiero.Business
{
    public static partial class Data
    {
        public static class UI
        {
            public static readonly GameDatum<int> TileSize = new(nameof(UI) + nameof(TileSize));
            public static readonly GameDatum<Coord> WindowSize = new(nameof(UI) + nameof(WindowSize));
            public static readonly GameDatum<SFML.Graphics.Color> DefaultActiveColor = new(nameof(UI) + nameof(DefaultActiveColor));
            public static readonly GameDatum<SFML.Graphics.Color> DefaultInactiveColor = new(nameof(UI) + nameof(DefaultInactiveColor));
        }

    }
}
