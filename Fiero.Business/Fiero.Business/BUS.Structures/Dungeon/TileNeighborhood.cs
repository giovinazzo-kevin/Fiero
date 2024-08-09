namespace Fiero.Business
{
    public readonly record struct TileRule(string Tile, bool Different = false)
    {
        public static implicit operator TileRule(string tile) => new(tile, false);
    }


    public readonly struct TileNeighborhood(
        TileRule tl, TileRule t, TileRule tr,
        TileRule l, TileRule m, TileRule r,
        TileRule bl, TileRule b, TileRule br
    )
    {
        public readonly TileRule[] Tiles = [tl, t, tr, l, m, r, bl, b, br];
        public readonly TileRule Topleft => Tiles[0];
        public readonly TileRule Top => Tiles[1];
        public readonly TileRule TopRight => Tiles[2];
        public readonly TileRule Left => Tiles[3];
        public readonly TileRule Middle => Tiles[4];
        public readonly TileRule Right => Tiles[5];
        public readonly TileRule BottomLeft => Tiles[6];
        public readonly TileRule Bottom => Tiles[7];
        public readonly TileRule BottomRight => Tiles[8];

        public bool Matches(TileNeighborhood other)
        {
            for (int i = 0; i < Tiles.Length; i++)
            {
                if (Tiles[i].Tile is null || other.Tiles[i].Tile is null)
                    continue;
                if (!Tiles[i].Different && Tiles[i].Tile != other.Tiles[i].Tile)
                    return false;
                if (Tiles[i].Different && Tiles[i].Tile == other.Tiles[i].Tile)
                    return false;
            }
            return true;
        }
    }
}
