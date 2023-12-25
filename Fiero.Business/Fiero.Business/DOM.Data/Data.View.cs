using SFML.Graphics;

namespace Fiero.Business
{
    public static partial class Data
    {
        public static class View
        {
            public static readonly GameDatum<int> TileSize = new(nameof(View) + nameof(TileSize));
            public static readonly GameDatum<Coord> MinWindowSize = new(nameof(View) + nameof(MinWindowSize));
            public static readonly GameDatum<Coord> WindowSize = new(nameof(View) + nameof(WindowSize));
            public static readonly GameDatum<Coord> ViewportSize = new(nameof(View) + nameof(ViewportSize));
            public static readonly GameDatum<Coord> PopUpSize = new(nameof(View) + nameof(PopUpSize));
            public static readonly GameDatum<Color> DefaultForeground = new(nameof(View) + nameof(DefaultForeground));
            public static readonly GameDatum<Color> DefaultBackground = new(nameof(View) + nameof(DefaultBackground));
            public static readonly GameDatum<Color> DefaultAccent = new(nameof(View) + nameof(DefaultAccent));
        }

    }
}
