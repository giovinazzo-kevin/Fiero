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
            public static readonly GameDatum<SFML.Graphics.Color> DefaultForeground = new(nameof(UI) + nameof(DefaultForeground));
            public static readonly GameDatum<SFML.Graphics.Color> DefaultBackground = new(nameof(UI) + nameof(DefaultBackground));
            public static readonly GameDatum<SFML.Graphics.Color> DefaultAccent = new(nameof(UI) + nameof(DefaultAccent));
        }

    }
}
