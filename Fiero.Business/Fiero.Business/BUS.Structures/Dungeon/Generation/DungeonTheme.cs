using Fiero.Core;
using System;
using System.Collections.Generic;

namespace Fiero.Business
{
    public readonly record struct DungeonTheme(
        Func<Coord, TileDef> WallTile,
        Func<Coord, TileDef> RoomTile,
        Func<Coord, TileDef> CorridorTile,
        Func<Coord, TileDef> WaterTile,
        Func<Coord, TileDef> HoleTile,
        Func<GameEntityBuilders, Coord, EntityBuilder<Feature>> DoorFeature,
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
            WaterTile: c => new(TileName.Water, c, ColorName.LightBlue),
            HoleTile: c => new(TileName.Hole, c),
            DoorFeature: (e, c) => e.Feature_Door(),
            Rules: new() {
                // TODO: Replace with a graphical rule that doesn't actually write to the tile context
                new((ctx, t) => t.Name == TileName.Hole && ctx.TryGetTile(t.Position - Coord.PositiveY, out var above) && above.Name == TileName.Hole,
                    (ctx, t) => t.WithTileName(TileName.None))
            },
            CorridorThickness: new(1, 3, (die, side) => 1f / Math.Pow(side, 2)), // thick corridors are rarer
            SecretCorridors: new(0, 0),
            RoomSquares: new(1, 6),
            DoorChance: Chance.FiftyFifty,
            UnevenCorridors: true
        );
    }
}
