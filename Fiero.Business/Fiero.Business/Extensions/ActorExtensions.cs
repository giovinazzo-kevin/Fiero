using Fiero.Core;
using System.Drawing;
using System.Net.Http.Headers;

namespace Fiero.Business
{

    public static class ActorExtensions
    {
        public static bool IsHostileTowards(this Actor a, Actor b)
        {
            if (a.ActorProperties.Relationships.TryGet(b, out var standing)) {
                return standing.MayTarget();
            }
            return a.Faction.Relationships.Get(b.Faction.Type).MayTarget();
        }

        public static bool IsFriendlyTowards(this Actor a, Actor b)
        {
            if (a.ActorProperties.Relationships.TryGet(b, out var standing)) {
                return standing.MayHelp();
            }
            return a.Faction.Relationships.Get(b.Faction.Type).MayHelp();
        }

        public static FloorId FloorId(this Actor a) => a.ActorProperties.FloorId;
        public static bool CanSee(this Actor a, Coord c) => a.Fov?.VisibleTiles[a.FloorId()].Contains(c) ?? true;
        public static bool CanSee(this Actor a, Drawable e) => a.CanSee(e.Physics.Position);
    }
}
