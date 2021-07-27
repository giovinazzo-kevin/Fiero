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
        public static double DistanceFrom(this PhysicalEntity a, PhysicalEntity b)
            => a.DistanceFrom(b.Position());
        public static double SquaredDistanceFrom(this PhysicalEntity a, PhysicalEntity b)
            => a.SquaredDistanceFrom(b.Position());
        public static double DistanceFrom(this PhysicalEntity a, Coord pos)
            => a.Position().Dist(pos);
        public static double SquaredDistanceFrom(this PhysicalEntity a, Coord pos)
            => a.Position().DistSq(pos);
        public static bool IsHostileTowards(this Actor a, Actor b)
        {
            if (a.Faction.PersonalRelationships.TryGet(b, out var standing)) {
                return standing.MayTarget();
            }
            return a.Faction.FactionRelationships.Get(b.Faction.Type).MayTarget();
        }

        public static bool IsFriendlyTowards(this Actor a, Actor b)
        {
            if (a.Faction.PersonalRelationships.TryGet(b, out var standing)) {
                return standing.MayHelp();
            }
            return a.Faction.FactionRelationships.Get(b.Faction.Type).MayHelp();
        }

        public static FloorId FloorId(this PhysicalEntity a) => a.Physics.FloorId;
        public static Coord Position(this PhysicalEntity a) => a.Physics.Position;
        public static bool IsPlayer(this Actor a) => a.ActorProperties.Type == ActorName.Player;
        public static bool CanSee(this Actor a, Coord c) => a.Fov != null && a.Fov.VisibleTiles.TryGetValue(a.FloorId(), out var tiles) && tiles.Contains(c);
        public static bool CanSee(this Actor a, PhysicalEntity e) => a.CanSee(e.Position());
        public static int Heal(this Actor a, int health)
        {
            return a.ActorProperties.Stats.Health = 
                Math.Clamp(a.ActorProperties.Stats.Health + health, 0, a.ActorProperties.Stats.MaximumHealth);
        }
    }
}
