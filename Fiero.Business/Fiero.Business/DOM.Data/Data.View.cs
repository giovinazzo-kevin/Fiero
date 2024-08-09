using SFML.Graphics;

namespace Fiero.Business
{
    public static partial class Data
    {
        public static class View
        {
            public static readonly GameDatum<int> TileSize = new(nameof(View), nameof(TileSize));
            public static readonly GameDatum<Coord> ViewportSize = new(nameof(View), nameof(ViewportSize));
            public static readonly GameDatum<Coord> PopUpSize = new(nameof(View), nameof(PopUpSize));
        }

    }
}
