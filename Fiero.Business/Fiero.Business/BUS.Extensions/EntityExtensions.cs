using Fiero.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http.Headers;

namespace Fiero.Business
{

    public static class EntityExtensions
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

        public static FloorId FloorId(this Drawable a) => a.Physics.FloorId;
        public static bool IsPlayer(this Actor a) => a.ActorProperties.Type == ActorName.Player;
        public static bool CanSee(this Actor a, Coord c) => a.Fov != null && a.Fov.VisibleTiles.TryGetValue(a.FloorId(), out var tiles) && tiles.Contains(c);
        public static bool CanSee(this Actor a, Drawable e) => a.CanSee(e.Physics.Position);
        public static int Heal(this Actor a, int health)
        {
            return a.ActorProperties.Stats.Health = 
                Math.Clamp(a.ActorProperties.Stats.Health + health, 0, a.ActorProperties.Stats.MaximumHealth);
        }
    }
}
