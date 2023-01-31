using Fiero.Core;
using Fiero.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{

    public partial class AiActionProvider : ActionProvider
    {
        protected readonly List<IAISensor> Sensors;

        protected readonly AiSensor<Stat> MyHealth;
        protected readonly AiSensor<Weapon> MyWeapons;
        protected readonly AiSensor<Consumable> MyConsumables;
        protected readonly AiSensor<Consumable> MyHelpfulConsumables;
        protected readonly AiSensor<Consumable> MyHarmfulConsumables;
        protected readonly AiSensor<Consumable> MyUnidentifiedConsumables;
        protected readonly AiSensor<Consumable> MyPanicButtons;

        protected readonly AiSensor<Item> NearbyItems;
        protected readonly AiSensor<Actor> NearbyEnemies;
        protected readonly AiSensor<Actor> NearbyAllies;

        protected int TurnsSinceSightingHostile { get; private set; }
        protected int TurnsSinceSightingFriendly { get; private set; }

        protected int RepathOneTimeIn { get; set; } = 25;

        protected bool Panic => MyHealth.RaisingAlert && TurnsSinceSightingHostile < 10;


        private StateName _state = StateName.Wandering;

        public AiActionProvider(GameSystems systems)
            : base(systems)
        {
            Sensors = new() {
                (MyHealth = new((sys, a) => new[] { a.ActorProperties.Health })),
                (MyWeapons = new((sys, a) => a.Inventory.GetWeapons()
                    .OrderByDescending(w => w.WeaponProperties.DamagePerTurn))),
                (MyConsumables = new((sys, a) => a.Inventory.GetConsumables()
                    .Where(v => v.ItemProperties.Identified))),
                (MyUnidentifiedConsumables = new((sys, a) => a.Inventory.GetConsumables()
                    .Where(v => !v.ItemProperties.Identified))),
                (MyHelpfulConsumables = new((sys, a) => MyConsumables.AlertingValues
                    .Where(v => v.GetEffectFlags().IsDefensive))),
                (MyHarmfulConsumables = new((sys, a) => MyConsumables.AlertingValues
                    .Where(v => v.TryCast<Throwable>(out var t) && t.ThrowableProperties.BaseDamage > 0
                             || v.GetEffectFlags().IsOffensive))),
                (MyPanicButtons = new((sys, a) => MyConsumables.AlertingValues
                    .Where(v => v.GetEffectFlags().IsPanicButton))),
                (NearbyAllies = new((sys, a) => {
                    return Shapes.Neighborhood(a.Position(), 7)
                     .SelectMany(p => sys.Dungeon.GetActorsAt(a.FloorId(), p))
                     .Where(b => sys.Faction.GetRelations(a, b).Right.IsFriendly() && a.CanSee(b));
                })),
                (NearbyEnemies = new((sys, a) => {
                    return Shapes.Neighborhood(a.Position(), 7)
                     .SelectMany(p => sys.Dungeon.GetActorsAt(a.FloorId(), p))
                     .Where(b => sys.Faction.GetRelations(a, b).Right.IsHostile() && a.CanSee(b)
                        && NearbyAllies.Values.Count(v => v.Ai != null && v.Ai.Target == b) < 3);
                })),
                (NearbyItems = new((sys, a) => {
                    return Shapes.Box(a.Position(), 7)
                     .SelectMany(p => sys.Dungeon.GetItemsAt(a.FloorId(), p))
                     .OrderBy(i => i.SquaredDistanceFrom(a));
                }))
            };

            MyHealth.ConfigureAlert((s, a, v) => v.Percentage <= 0.25f);
            MyConsumables.ConfigureAlert((s, a, v) => v.ConsumableProperties.RemainingUses > 0);
            MyWeapons.ConfigureAlert((s, a, v) => a.Equipment.Weapon is null || v.WeaponProperties.DamagePerTurn > a.Equipment.Weapon.WeaponProperties.DamagePerTurn);
            NearbyItems.ConfigureAlert((s, a, v) => !a.Inventory.Full && a.Ai.LikedItems.Any(f => f(v)));
        }

        protected virtual StateName UpdateState(StateName state)
        {
            if (Panic)
            {
                return StateName.Retreating;
            }
            if (NearbyEnemies.Values.Count == 0)
            {
                return StateName.Wandering;
            }
            return StateName.Fighting;
        }

        protected Actor GetClosestHostile(Actor a) => NearbyEnemies.Values
            .OrderBy(b => a.SquaredDistanceFrom(b))
            .FirstOrDefault();


        protected Actor GetClosestFriendly(Actor a) => NearbyAllies.Values
            .OrderBy(b => a.SquaredDistanceFrom(b))
            .FirstOrDefault();


        protected virtual bool TryUseItem(Actor a, Item item, out IAction action)
        {
            action = default;
            var flags = item.GetEffectFlags();
            if (item.TryCast<Potion>(out var potion) && flags.IsDefensive && Panic)
            {
                action = new QuaffPotionAction(potion);
                return true;
            }
            if (item.TryCast<Scroll>(out var scroll))
            {
                action = new ReadScrollAction(scroll);
                return true;
            }
            if (item.TryCast<Wand>(out var wand))
            {
                return TryZap(a, wand, out action);
            }
            if (item.TryCast<Throwable>(out var throwable))
            {
                return TryThrow(a, throwable, out action);
            }
            return false;
        }

        public override bool TryTarget(Actor a, TargetingShape shape, bool autotargetSuccesful)
        {
            return autotargetSuccesful && shape.GetPoints().Any(p => Systems.Dungeon.GetActorsAt(a.FloorId(), p).Any());
        }

        protected virtual void SetTarget(Actor a, PhysicalEntity target, Func<IAction> goal = null)
        {
            a.Ai.Target = target;
            a.Ai.Goal = goal;
            TryRecalculatePath(a);
        }

        protected virtual bool TryRecalculatePath(Actor a)
        {
            if (a.Ai.Path != null && a.Ai.Path.Last != null && a.Ai.Path.Last.Value.Tile.Position() == a.Position())
                return false;
            var floor = Systems.Dungeon.GetFloor(a.FloorId());
            a.Ai.Path = floor.Pathfinder.Search(a.Position(), a.Ai.Target.Position(), a);
            a.Ai.Path?.RemoveFirst();
            return true;
        }

        protected virtual bool TryFollowPath(Actor a, out IAction action)
        {
            action = default;
            if (a.Ai.Path != null && a.Ai.Path.First != null)
            {
                var pos = a.Ai.Path.First.Value.Tile.Position();
                var dir = new Coord(pos.X - a.Position().X, pos.Y - a.Position().Y);
                var diff = Math.Abs(dir.X) + Math.Abs(dir.Y);
                a.Ai.Path.RemoveFirst();
                if (diff > 0 && diff <= 2)
                {
                    // one tile ahead
                    if (Systems.Dungeon.TryGetCellAt(a.FloorId(), pos, out var cell)
                        && cell.IsWalkable(a) && !cell.Actors.Any())
                    {
                        action = new MoveRelativeAction(dir);
                        return true;
                    }
                }
            }
            // Destination was reached
            else if (a.Ai.Path != null && a.Ai.Path.First == null && a.Ai.Goal is { } goal)
            {
                Clear();
                action = goal();
                return true;
            }
            // Path recalculation is necessary
            Clear();
            return false;

            void Clear()
            {
                a.Ai.Path = null;
                a.Ai.Target = null;
                a.Ai.Goal = null;
            }
        }

        protected virtual IAction Retreat(Actor a)
        {
            if (MyPanicButtons.Values.Count > 0)
            {
                foreach (var item in MyPanicButtons.Values.Shuffle(Rng.Random))
                {
                    if (TryUseItem(a, item, out var action))
                    {
                        return action;
                    }
                }
            }
            if (GetClosestHostile(a) is { } hostile)
            {
                var dir = a.Position() - hostile.Position();
                if (!Systems.Dungeon.TryGetCellAt(a.FloorId(), a.Position() + dir, out var cell))
                {
                    return Fight(a);
                }
                if (!cell.IsWalkable(a))
                {
                    return Fight(a);
                }
                return new MoveRelativeAction(dir);
            }
            return new WaitAction();
        }

        protected virtual IAction Fight(Actor a)
        {
            IAction action;
            if (GetClosestHostile(a) is { } hostile)
            {
                SetTarget(a, hostile);
                if (a.IsInMeleeRange(hostile))
                {
                    return new MeleeAttackOtherAction(hostile, a.Equipment.Weapon);
                }

                if (MyWeapons.AlertingValues.FirstOrDefault() is { } betterWeapon)
                {
                    return new EquipItemAction(betterWeapon);
                }
                if (NearbyEnemies.Values.Count > 0 && MyHarmfulConsumables.Values.Count > 0 && Rng.Random.NChancesIn(2, 3))
                {
                    foreach (var item in MyHarmfulConsumables.Values.Shuffle(Rng.Random))
                    {
                        if (TryUseItem(a, item, out action))
                        {
                            return action;
                        }
                    }
                }
                if (NearbyAllies.Values.Count > 0 && MyHelpfulConsumables.Values.Count > 0 && Rng.Random.NChancesIn(1, 2))
                {
                    foreach (var item in MyHelpfulConsumables.Values.Shuffle(Rng.Random))
                    {
                        if (TryUseItem(a, item, out action))
                        {
                            return action;
                        }
                    }
                }
                if (MyUnidentifiedConsumables.Values.Count > 0 && Rng.Random.NChancesIn(1, 4))
                {
                    foreach (var item in MyUnidentifiedConsumables.Values.Shuffle(Rng.Random))
                    {
                        if (TryUseItem(a, item, out action))
                        {
                            return action;
                        }
                    }
                }

                if (TryFollowPath(a, out action))
                {
                    return action;
                }
            }
            return new WaitAction();
        }

        protected virtual IAction Wander(Actor a)
        {
            if (NearbyItems.AlertingValues.Count > 0)
            {
                var closestItem = NearbyItems.AlertingValues.First();
                if (a.Position() == closestItem.Position())
                {
                    return new PickUpItemAction(closestItem);
                }
                SetTarget(a, closestItem);
            }
            if (TryFollowPath(a, out var action))
            {
                return action;
            }
            if (a.Ai.Target == null && Rng.Random.NChancesIn(1, RepathOneTimeIn))
            {
                var randomTile = Systems.Dungeon.GetFloor(a.FloorId())
                    .Cells.Values.Where(c => c.IsWalkable(a) && !a.Knows(c.Tile.Position()));
                if (randomTile.Any())
                {
                    SetTarget(a, randomTile.Shuffle(Rng.Random).First().Tile);
                }
            }
            return new MoveRandomlyAction();
        }

        protected virtual void UpdateCounters()
        {
            if (NearbyEnemies.Values.Count == 0)
            {
                TurnsSinceSightingHostile++;
            }
            else
            {
                TurnsSinceSightingHostile = 0;
            }
            if (NearbyAllies.Values.Count == 0)
            {
                TurnsSinceSightingFriendly++;
            }
            else
            {
                TurnsSinceSightingFriendly = 0;
            }
        }

        public override IAction GetIntent(Actor a)
        {
            foreach (var sensor in Sensors)
            {
                sensor.Update(Systems, a);
            }
            UpdateCounters();
            return (_state = UpdateState(_state)) switch
            {
                StateName.Retreating => Retreat(a),
                StateName.Fighting => Fight(a),
                StateName.Wandering => Wander(a),
                _ => throw new NotSupportedException(_state.ToString()),
            };
        }
    }
}


/*
 

            var floorId = a.FloorId();
            if (!Systems.Floor.TryGetFloor(floorId, out var floor))
                throw new ArgumentException(nameof(floorId));

            if (a.Ai.Target is null || !a.Ai.Target.IsAlive()) {
                a.Ai.Target = null; // invalidation
            }
            if (a.Ai.Target == null) {
                // Seek new target to attack
                if (!a.Fov.VisibleTiles.TryGetValue(floorId, out var fov)) {
                    return new MoveRandomlyAction();
                }
                var target = Systems.Faction.GetRelations(a)
                    .Where(r => r.Standing.IsHostile() && fov.Contains(r.Actor.Position()))
                    .Select(r => r.Actor)
                    .FirstOrDefault()
                    ?? fov.SelectMany(c => Systems.Floor.GetActorsAt(floorId, c))
                    .FirstOrDefault(b => Systems.Faction.GetRelations(a, b).Left.IsHostile());
                if (target != null) {
                    a.Ai.Target = target;
                }
            }
            if (a.Ai.Target != null) {
                if (a.Ai.Target.DistanceFrom(a) < 2) {
                    return new MeleeAttackOtherAction(a.Ai.Target, a.Equipment.Weapon);
                }
                if (a.CanSee(a.Ai.Target)) {
                    // If we can see the target and it has moved, recalculate the path
                    a.Ai.Path = floor.Pathfinder.Search(a.Position(), a.Ai.Target.Position(), default);
                    a.Ai.Path?.RemoveFirst();
                }
            }
            // Path to a random tile
            if (a.Ai.Path == null && Rng.Random.OneChanceIn(5)) {
                var randomTile = floor.Cells.Values.Shuffle(Rng.Random).Where(c => c.Tile.TileProperties.Name == TileName.Room
                    && c.IsWalkable(null)).First();
                a.Ai.Path = floor.Pathfinder.Search(a.Position(), randomTile.Tile.Position(), default);
            }
            // If following a path, do so until the end or an obstacle is reached
            else if (a.Ai.Path != null) {
                if (a.Ai.Path.First != null) {
                    var pos = a.Ai.Path.First.Value.Tile.Position();
                    var dir = new Coord(pos.X - a.Position().X, pos.Y - a.Position().Y);
                    var diff = Math.Abs(dir.X) + Math.Abs(dir.Y);
                    a.Ai.Path.RemoveFirst();
                    if (diff > 0 && diff <= 2) {
                        // one tile ahead
                        return new MoveRelativeAction(dir);
                    }
                }
                else {
                    a.Ai.Path = null;
                    return GetIntent(a);
                }
            }
            return new MoveRandomlyAction();
 
 */