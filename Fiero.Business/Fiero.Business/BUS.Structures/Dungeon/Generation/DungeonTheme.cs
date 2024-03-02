namespace Fiero.Business
{
    public readonly record struct DungeonTheme(
        Func<Coord, TileDef> WallTile,
        Func<Coord, TileDef> RoomTile,
        Func<Coord, TileDef> CorridorTile,
        Func<Coord, TileDef> ShopTile,
        Func<Coord, TileDef> WaterTile,
        Func<Coord, TileDef> HoleTile,
        Func<GameEntityBuilders, Coord, IEntityBuilder<Feature>> DoorFeature,
        List<FloorGenerationRule> Rules,
        Dice CorridorThickness,
        Dice RoomSquares,
        Dice SecretCorridors,
        Chance DoorChance,
        bool UnevenCorridors

    )
    {
        public static readonly DungeonTheme Default = new DungeonTheme(
            WallTile: c => new(TileName.Wall, c),
            RoomTile: c => new(TileName.Room, c),
            CorridorTile: c => new(TileName.Corridor, c),
            ShopTile: c => new(TileName.Shop, c),
            WaterTile: c => new(TileName.Water, c, ColorName.LightBlue),
            HoleTile: c => new(TileName.WallFront, c),
            DoorFeature: (e, c) => e.Feature_Door(),
            Rules: new() {
                // TODO: Replace with a graphical rule that doesn't actually write to the tile context
                new((ctx, t) => t.Name == TileName.Wall && (!ctx.TryGetTile(t.Position + Coord.PositiveY, out var below) || below.Name != TileName.Wall),
                    (ctx, t) => t.WithTileName(TileName.WallFront))
            },
            CorridorThickness: new(1, 3, (die, side) => 1f / Math.Pow(side, 2)), // thick corridors are rarer
            SecretCorridors: new(0, 0),
            RoomSquares: new(1, 6),
            DoorChance: Chance.FiftyFifty,
            UnevenCorridors: true
        );
    }
}
