using Fiero.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
        public static FloorId FloorId(this PhysicalEntity a) => a.Physics.FloorId;
        public static Coord Position(this PhysicalEntity a) => a.Physics.Position;
        public static bool IsInvalid(this Entity e) => e is null || e.Id == 0;
        public static bool IsAlive(this PhysicalEntity a) => !a.IsInvalid() && a.FloorId() != default;
        public static bool TryRoot(this PhysicalEntity a)
        {
            var ret = a.IsAlive() && !a.IsRooted();
            if(ret) {
                ++a.Physics.Roots;
            }
            return ret;
        }
        public static bool TryFree(this PhysicalEntity a)
        {
            var ret = a.IsAlive() && a.IsRooted();
            if (ret) {
                --a.Physics.Roots;
            }
            return ret;
        }
        public static bool IsRooted(this PhysicalEntity a) => a.IsAlive() && a.Physics.Roots > 0;
        public static bool IsImmobile(this PhysicalEntity a) => a.IsAlive() && !a.Physics.CanMove || a.Physics.Roots > 0;
        public static bool IsPlayer(this Actor a) => a.IsAlive() && a.ActorProperties.Type == ActorName.Player;
        public static bool CanSee(this Actor a, Coord c) => a.IsAlive() && a?.Fov != null && a.Fov.VisibleTiles.TryGetValue(a.FloorId(), out var tiles) && tiles.Contains(c);
        public static bool CanSee(this Actor a, PhysicalEntity e) => a.IsAlive() && e != null && a.CanSee(e.Position());
        public static bool IsAffectedBy(this Actor a, EffectName effect) => a.IsAlive() && a.Effects != null && a.Effects.Active.Any(e => e.Name == effect);
        public static int Heal(this Actor a, int health)
        {
            return a.ActorProperties.Stats.Health = 
                Math.Clamp(a.ActorProperties.Stats.Health + health, 0, a.ActorProperties.Stats.MaximumHealth);
        }
        public static int Damage(this Actor a, int health) => a.Heal(-health);

        public static bool TryIdentify(this Actor a, Item i) => a.Inventory.TryIdentify(i);
        public static bool Identify<T>(this Actor a, T i, Func<T, bool> rule)
            where T : Item
        {
            if (!rule(i))
                throw new ArgumentException(nameof(rule));
            if(!a.TryIdentify(i)) {
                a.Inventory.AddIdentificationRule(i => i is T _t && rule(_t) || i.TryCast<T>(out var t) && rule(t));
                foreach (var other in a.Inventory.GetItems().Where(i => !i.ItemProperties.Identified)) {
                    a.TryIdentify(other);
                }
                return a.TryIdentify(i);
            }
            return false;
        }
    }
}
