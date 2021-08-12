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
        protected readonly Dictionary<OrderedPair<FactionName>, StandingName> FactionRelationships = new();
        protected readonly Dictionary<OrderedPair<int>, StandingName> ActorRelationships = new();

        public void SetDefaultRelationships()
        {
            SetBilateralRelationship(FactionName.Rats, FactionName.Cats, StandingName.Tolerated);
            SetBilateralRelationship(FactionName.Rats, FactionName.Dogs, StandingName.Tolerated);
            SetBilateralRelationship(FactionName.Rats, FactionName.Snakes, StandingName.Tolerated);
            SetBilateralRelationship(FactionName.Rats, FactionName.Boars, StandingName.Tolerated);
            SetBilateralRelationship(FactionName.Cats, FactionName.Dogs, StandingName.Tolerated);
            SetBilateralRelationship(FactionName.Cats, FactionName.Snakes, StandingName.Tolerated);
            SetBilateralRelationship(FactionName.Cats, FactionName.Boars, StandingName.Tolerated);
            SetBilateralRelationship(FactionName.Dogs, FactionName.Snakes, StandingName.Tolerated);
            SetBilateralRelationship(FactionName.Dogs, FactionName.Boars, StandingName.Tolerated);
            SetBilateralRelationship(FactionName.Snakes, FactionName.Boars, StandingName.Tolerated);

            SetBilateralRelationship(FactionName.Players, FactionName.Rats, StandingName.DisapprovenOf);
            SetBilateralRelationship(FactionName.Players, FactionName.Cats,StandingName.DisapprovenOf);
            SetBilateralRelationship(FactionName.Players, FactionName.Dogs, StandingName.DisapprovenOf);
            SetBilateralRelationship(FactionName.Players, FactionName.Snakes, StandingName.DisapprovenOf);
            SetBilateralRelationship(FactionName.Players, FactionName.Boars, StandingName.DisapprovenOf);
            SetBilateralRelationship(FactionName.Players, FactionName.BeastGod, StandingName.DisapprovenOf);
        }

        public void SetUnilateralRelationship(FactionName a, FactionName b, StandingName standing)
        {
            FactionRelationships[new(a, b)] = standing;
        }

        public void SetBilateralRelationship(FactionName a, FactionName b, StandingName standing)
        {
            SetUnilateralRelationship(a, b, standing);
            SetUnilateralRelationship(b, a, standing);
        }

        public void SetUnilateralRelationship(Actor a, Actor b, StandingName standing)
        {
            if (!a.IsAlive()) {
                throw new ArgumentException(nameof(a));
            }
            if (!b.IsAlive()) {
                throw new ArgumentException(nameof(b));
            }
            ActorRelationships[new(a.Id, b.Id)] = standing;
        }

        public void SetBilateralRelationship(Actor a, Actor b, StandingName standing)
        {
            SetUnilateralRelationship(a, b, standing);
            SetUnilateralRelationship(b, a, standing);
        }

        public OrderedPair<StandingName> GetRelationships(FactionName a, FactionName b)
        {
            if (!FactionRelationships.TryGetValue(new(a, b), out var aTowardsB)) {
                aTowardsB = StandingName.Tolerated;
            }
            if (!FactionRelationships.TryGetValue(new(b, a), out var bTowardsA)) {
                bTowardsA = StandingName.Tolerated;
            }
            return new(aTowardsB, bTowardsA);
        }

        public OrderedPair<StandingName> GetRelationships(Actor a, Actor b)
        {
            if (!ActorRelationships.TryGetValue(new(a.Id, b.Id), out var aTowardsB)) {
                if (!FactionRelationships.TryGetValue(new(a.Faction.Name, b.Faction.Name), out aTowardsB)) {
                    aTowardsB = StandingName.Tolerated;
                }
            }
            if (!ActorRelationships.TryGetValue(new(b.Id, a.Id), out var bTowardsA)) {
                if (!FactionRelationships.TryGetValue(new(b.Faction.Name, a.Faction.Name), out bTowardsA)) {
                    bTowardsA = StandingName.Tolerated;
                }
            }
            return new(aTowardsB, bTowardsA);
        }

        public IEnumerable<(FactionName Faction, StandingName Standing)> GetRelationships(FactionName f)
        {
            var keys = FactionRelationships.Keys.Where(k => k.Left == f);
            foreach (var key in keys) {
                yield return (key.Right, FactionRelationships[key]);
            }
        }

        public IEnumerable<(Actor Actor, StandingName Standing)> GetRelationships(Actor a)
        {
            var keys = ActorRelationships.Keys.Where(k => k.Left == a.Id);
            foreach (var key in keys) {
                if (!Entities.TryGetProxy<Actor>(key.Right, out var other)) {
                    ActorRelationships.Remove(key);
                    continue;
                }
                yield return (other, ActorRelationships[key]);
            }
        }

        public FactionSystem(EventBus bus, GameEntities entities)
            : base(bus)
        {
            Entities = entities;
        }
    }
}
