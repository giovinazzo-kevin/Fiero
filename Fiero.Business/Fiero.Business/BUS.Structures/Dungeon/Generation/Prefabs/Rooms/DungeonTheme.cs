using Fiero.Core;
using System;

namespace Fiero.Business
{
    public readonly record struct DungeonTheme(
        Func<Coord, TileDef> WallTile,
        Func<Coord, TileDef> RoomTile,
        Func<Coord, TileDef> CorridorTile,
        Func<Coord, TileDef> WaterTile,
        Func<GameEntityBuilders, Coord, EntityBuilder<Feature>> DoorFeature,
        Dice CorridorThickness,
        Dice MaxRoomSquares,
        Chance DoorChance,
        bool UnevenCorridors

    )
    {
        public static readonly DungeonTheme Default = new DungeonTheme(
            WallTile: c => new(TileName.Wall, c),
            RoomTile: c => new(TileName.Room, c),
            CorridorTile: c => new(TileName.Corridor, c),
            WaterTile: c => new(TileName.Water, c, ColorName.LightBlue),
            DoorFeature: (e, c) => e.Feature_Door(),
            CorridorThickness: new(1, 3, (die, side) => 1f / Math.Pow(side, 2)), // thick corridors are rarer
            MaxRoomSquares: new(1, 6),
            DoorChance: Chance.FiftyFifty,
            UnevenCorridors: true
        );
    }
}
