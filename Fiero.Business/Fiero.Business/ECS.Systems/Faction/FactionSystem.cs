using Fiero.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Unconcern.Common;

namespace Fiero.Business
{
    public class FactionSystem : EcsSystem
    {
        public readonly GameEntities Entities;
        protected readonly Dictionary<OrderedPair<FactionName>, StandingName> FactionRelations = new();
        protected readonly Dictionary<OrderedPair<int>, StandingName> ActorRelations = new();

        public void SetDefaultRelations()
        {
            SetBilateralRelation(FactionName.Rats, FactionName.Cats, StandingName.Tolerated);
            SetBilateralRelation(FactionName.Rats, FactionName.Dogs, StandingName.Tolerated);
            SetBilateralRelation(FactionName.Rats, FactionName.Snakes, StandingName.Tolerated);
            SetBilateralRelation(FactionName.Rats, FactionName.Boars, StandingName.Tolerated);
            SetBilateralRelation(FactionName.Cats, FactionName.Dogs, StandingName.Tolerated);
            SetBilateralRelation(FactionName.Cats, FactionName.Snakes, StandingName.Tolerated);
            SetBilateralRelation(FactionName.Cats, FactionName.Boars, StandingName.Tolerated);
            SetBilateralRelation(FactionName.Dogs, FactionName.Snakes, StandingName.Tolerated);
            SetBilateralRelation(FactionName.Dogs, FactionName.Boars, StandingName.Tolerated);
            SetBilateralRelation(FactionName.Snakes, FactionName.Boars, StandingName.Tolerated);

            SetBilateralRelation(FactionName.Players, FactionName.Rats, StandingName.DisapprovenOf);
            SetBilateralRelation(FactionName.Players, FactionName.Cats, StandingName.DisapprovenOf);
            SetBilateralRelation(FactionName.Players, FactionName.Dogs, StandingName.DisapprovenOf);
            SetBilateralRelation(FactionName.Players, FactionName.Snakes, StandingName.DisapprovenOf);
            SetBilateralRelation(FactionName.Players, FactionName.Boars, StandingName.DisapprovenOf);
            SetBilateralRelation(FactionName.Players, FactionName.Monsters, StandingName.DisapprovenOf);
        }

        public void SetUnilateralRelation(FactionName a, FactionName b, StandingName standing)
        {
            FactionRelations[new(a, b)] = standing;
        }

        public void SetBilateralRelation(FactionName a, FactionName b, StandingName standing)
        {
            SetUnilateralRelation(a, b, standing);
            SetUnilateralRelation(b, a, standing);
        }

        public void SetUnilateralRelation(Actor a, Actor b, StandingName standing)
        {
            if (!a.IsAlive())
            {
                throw new ArgumentException(nameof(a));
            }
            if (!b.IsAlive())
            {
                throw new ArgumentException(nameof(b));
            }
            ActorRelations[new(a.Id, b.Id)] = standing;
        }

        public void SetBilateralRelation(Actor a, Actor b, StandingName standing)
        {
            SetUnilateralRelation(a, b, standing);
            SetUnilateralRelation(b, a, standing);
        }

        public OrderedPair<StandingName> GetRelations(FactionName a, FactionName b)
        {
            if (!FactionRelations.TryGetValue(new(a, b), out var aTowardsB))
            {
                aTowardsB = StandingName.Tolerated;
            }
            if (!FactionRelations.TryGetValue(new(b, a), out var bTowardsA))
            {
                bTowardsA = StandingName.Tolerated;
            }
            return new(aTowardsB, bTowardsA);
        }

        public OrderedPair<StandingName> GetRelations(Actor a, Actor b)
        {
            if (!ActorRelations.TryGetValue(new(a.Id, b.Id), out var aTowardsB))
            {
                if (!FactionRelations.TryGetValue(new(a.Faction.Name, b.Faction.Name), out aTowardsB))
                {
                    aTowardsB = StandingName.Tolerated;
                }
            }
            if (!ActorRelations.TryGetValue(new(b.Id, a.Id), out var bTowardsA))
            {
                if (!FactionRelations.TryGetValue(new(b.Faction.Name, a.Faction.Name), out bTowardsA))
                {
                    bTowardsA = StandingName.Tolerated;
                }
            }
            return new(aTowardsB, bTowardsA);
        }

        public IEnumerable<(FactionName Faction, StandingName Standing)> GetRelations(FactionName f)
        {
            var keys = FactionRelations.Keys.Where(k => k.Left == f);
            foreach (var key in keys)
            {
                yield return (key.Right, FactionRelations[key]);
            }
        }

        public IEnumerable<(Actor Actor, StandingName Standing)> GetRelations(Actor a)
        {
            var keys = ActorRelations.Keys.Where(k => k.Left == a.Id);
            foreach (var key in keys)
            {
                if (!Entities.TryGetProxy<Actor>(key.Right, out var other))
                {
                    ActorRelations.Remove(key);
                    continue;
                }
                yield return (other, ActorRelations[key]);
            }
        }

        public FactionSystem(EventBus bus, GameEntities entities)
            : base(bus)
        {
            Entities = entities;
        }
    }
}
