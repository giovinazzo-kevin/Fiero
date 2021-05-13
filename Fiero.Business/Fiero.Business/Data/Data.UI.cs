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
            public static readonly GameDatum<SFML.Graphics.Color> DefaultActiveForeground = new(nameof(UI) + nameof(DefaultActiveForeground));
            public static readonly GameDatum<SFML.Graphics.Color> DefaultInactiveForeground = new(nameof(UI) + nameof(DefaultInactiveForeground));
            public static readonly GameDatum<SFML.Graphics.Color> DefaultActiveBackground = new(nameof(UI) + nameof(DefaultActiveBackground));
            public static readonly GameDatum<SFML.Graphics.Color> DefaultInactiveBackground = new(nameof(UI) + nameof(DefaultInactiveBackground));
        }

    }
}
