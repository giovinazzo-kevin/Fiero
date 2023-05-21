using System.Linq;

namespace Fiero.Business
{
    public class SecretCorridor : Corridor
    {
        public SecretCorridor(RoomConnector a, RoomConnector b) : base(a, b) { }
        protected override DungeonTheme CustomizeTheme(DungeonTheme theme)
        {
            theme = base.CustomizeTheme(theme);
            return theme with
            {
                DoorFeature = (e, c) =>
                {
                    var wallColor = theme.WallTile(c).Color;
                    // Inherit theme from adjacent room
                    if (Start.Owner.GetConnectors().Any(x => x.Middle == c))
                        wallColor = Start.Owner.Theme.WallTile(c).Color;
                    else if (End.Owner.GetConnectors().Any(x => x.Middle == c))
                        wallColor = End.Owner.Theme.WallTile(c).Color;
                    return e.Feature_SecretDoor(wallColor ?? ColorName.White);
                },
                DoorChance = Chance.Always,
                CorridorThickness = new(1, 1)
            };
        }
    }
}
