using Fiero.Core;

namespace Fiero.Business
{
    public readonly struct TileDef
    {
        public readonly TileName Name;
        public readonly ColorName? Color;
        public readonly Coord Position;
        public TileDef(TileName name, Coord pos, ColorName? color = null) => (Name, Position, Color) = (name, pos, color);

        public TileDef WithCustomColor(ColorName c) => new(Name, Position, c);
        public TileDef WithDefaultColor() => new(Name, Position, null);
        public TileDef WithTileName(TileName newTile) => new(newTile, Position, Color);
        public TileDef WithPosition(Coord newPos) => new(Name, newPos, Color);
    }
}
