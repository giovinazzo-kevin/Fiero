using Fiero.Core;
using System.Drawing;
using System.Net.Http.Headers;

namespace Fiero.Business
{

    public static class ActorExtensions
    {
        public static bool IsHotileTowards(this Actor a, Actor b)
        {
            if (a.Properties.Standings.TryGet(b, out var standing)) {
                return standing.MayTarget();
            }
            return a.Faction.Standings.Get(b.Faction.Type).MayTarget();
        }

        public static bool IsFriendlyTowards(this Actor a, Actor b)
        {
            if (a.Properties.Standings.TryGet(b, out var standing)) {
                return standing.MayHelp();
            }
            return a.Faction.Standings.Get(b.Faction.Type).MayHelp();
        }

        public static bool CanSee(this Actor a, Drawable other) 
            => a.CanSee(other.Physics.Position);
        public static bool CanSee(this Actor a, Coord bPos)
        {
            var aPos = a.Physics.Position;
            return Utils.Bresenham((aPos.X, aPos.Y), (bPos.X, bPos.Y), (x, y) => {
                if (!a.Properties.CurrentFloor.Tiles.TryGetValue(new(x, y), out var tile)) {
                    return false;
                }
                if (tile.Properties.BlocksMovement) {
                    return false;
                }
                return true;
            });
        }
    }
}
