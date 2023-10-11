using Ergo.Lang;

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
            if (ret)
            {
                ++a.Physics.Roots;
            }
            return ret;
        }
        public static bool TryFree(this PhysicalEntity a)
        {
            var ret = a.IsAlive() && a.IsRooted();
            if (ret)
            {
                --a.Physics.Roots;
            }
            return ret;
        }

        public static bool IsDoorOpen(this Feature a) => a.FeatureProperties.Name == FeatureName.Door
            && !a.Physics.BlocksMovement;
        public static bool IsDoorClosed(this Feature a) => a.FeatureProperties.Name == FeatureName.Door
            && a.Physics.BlocksMovement;
        public static bool TryToggleDoor(this Feature a)
        {
            if (a is not { FeatureProperties: { Name: FeatureName.Door } } door)
                return false;
            door.Physics.BlocksPlayerPathing =
            door.Physics.BlocksNpcPathing =
            door.Physics.BlocksLight =
                (door.Physics.BlocksMovement ^= true);
            door.Render.Hidden = !door.Physics.BlocksMovement;
            return true;
        }
        public static bool TryJoinParty(this Actor a, Actor b)
        {
            if (a.Party is not { } aParty)
                return false;
            if (b.Party is not { } bParty)
                return false;
            if (bParty.Leader != null)
                return false;
            bParty.Followers.Add(a);
            aParty.Leader = b;
            return true;
        }
        public static bool TryLeaveParty(this Actor a, Actor b)
        {
            if (a.Party is not { } aParty)
                return false;
            if (b.Party is not { } bParty)
                return false;
            if (aParty.Leader != b)
                return false;
            bParty.Followers.Remove(a);
            aParty.Leader = null;
            return true;
        }
        public static bool TryOpenDoor(this Feature a)
        {
            if (a is not { FeatureProperties: { Name: FeatureName.Door } } door)
                return false;
            if (a.IsDoorOpen())
                return false;
            return a.TryToggleDoor();
        }
        public static bool TryCloseDoor(this Feature a)
        {
            if (a is not { FeatureProperties: { Name: FeatureName.Door } } door)
                return false;
            if (a.IsDoorClosed())
                return false;
            return a.TryToggleDoor();
        }
        public static bool IsInMeleeRange(this Actor a, Coord c) => a.SquaredDistanceFrom(c) <= 2;
        public static bool IsInMeleeRange(this Actor a, PhysicalEntity b) => a.IsInMeleeRange(b.Position());
        public static bool HasTrait(this Entity e, TraitName t) => e.Traits != null && e.Traits.Active.Any(x => x.Name == t);
        public static bool IsRooted(this PhysicalEntity a) => a.IsAlive() && a.Physics.Roots > 0;
        public static bool IsImmobile(this PhysicalEntity a) => a.IsAlive() && !a.Physics.CanMove || a.Physics.Roots > 0;
        public static bool IsPlayer(this Actor a) => a.IsAlive() && a.ActorProperties.Type == ActorName.Player;
        public static bool Knows(this Actor a, FloorId f, Coord c) => a.IsAlive() && a?.Fov != null && a.Fov.KnownTiles.TryGetValue(f, out var tiles) && tiles.Contains(c);
        public static bool CanSee(this Actor a, FloorId f, Coord c) => a.IsAlive() && a.FloorId() == f && a?.Fov != null && a.Fov.VisibleTiles.TryGetValue(f, out var tiles) && tiles.Contains(c);
        public static bool CanSee(this Actor a, Location l) =>
            CanSee(a, l.FloorId, l.Position);
        public static bool CanSeeEither(this Actor a, Either<Location, PhysicalEntity> l) =>
            l.Reduce(a.CanSee, a.CanSee);
        public static bool CanSee(this Actor a, PhysicalEntity e) => a.IsAlive() && !e.IsInvalid() && a.CanSee(e.FloorId(), e.Position()) && a.Fov.Sight.HasFlag(e.Render.Visibility);
        public static bool CanHear(this Actor a, PhysicalEntity e) => a.IsAlive() && !e.IsInvalid() && e.FloorId() == a.FloorId();
        public static bool IsAffectedBy(this Actor a, EffectName effect) => a.IsAlive() && a.Effects != null && a.Effects.Active.Any(e => e.Name == effect);
        public static bool TryIdentify(this Actor a, Item i) => a.Inventory != null && a.Inventory.TryIdentify(i);
        public static bool TryUseItem(this Actor actor, Item item, out bool consumed)
        {
            var used = false;
            consumed = false;
            if (item.TryCast<Consumable>(out var consumable))
            {
                used = TryConsume(out consumed);
            }
            if (consumed)
            {
                // Assumes item was used from inventory
                _ = actor.Inventory.TryTake(item);
            }
            return used;

            bool TryConsume(out bool consumed)
            {
                consumed = false;
                if (consumable.ConsumableProperties.RemainingUses <= 0)
                {
                    return false;
                }
                if (--consumable.ConsumableProperties.RemainingUses <= 0
                 && consumable.ConsumableProperties.ConsumedWhenEmpty)
                {
                    consumed = true;
                }
                return true;
            }
        }
        public static bool Identify<T>(this Actor a, T i, Func<T, bool> rule)
            where T : Item
        {
            if (!rule(i))
                throw new ArgumentException(nameof(rule));
            if (!a.TryIdentify(i))
            {
                a.Inventory.AddIdentificationRule(i => i is T _t && rule(_t) || i.TryCast<T>(out var t) && rule(t));
                foreach (var other in a.Inventory.GetItems().Where(j => j != i && !j.ItemProperties.Identified))
                {
                    a.TryIdentify(other);
                }
                return a.TryIdentify(i);
            }
            return false;
        }
    }
}
