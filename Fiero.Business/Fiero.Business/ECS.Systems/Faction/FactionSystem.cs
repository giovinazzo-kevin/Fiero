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
        protected readonly Dictionary<FactionName, FactionRelationships> Relationships;

        public FactionRelationships GetRelationships(FactionName faction) => Relationships[faction];

        public void SetDefaultRelationships()
        {
            SetMutualRelationship(FactionName.Rats, FactionName.Cats,
                new(StandingName.Tolerated));
            SetMutualRelationship(FactionName.Rats, FactionName.Dogs,
                new(StandingName.Tolerated));
            SetMutualRelationship(FactionName.Rats, FactionName.Snakes,
                new(StandingName.Tolerated));
            SetMutualRelationship(FactionName.Rats, FactionName.Boars,
                new(StandingName.Tolerated));
            SetMutualRelationship(FactionName.Cats, FactionName.Dogs,
                new(StandingName.Tolerated));
            SetMutualRelationship(FactionName.Cats, FactionName.Snakes,
                new(StandingName.Tolerated));
            SetMutualRelationship(FactionName.Cats, FactionName.Boars,
                new(StandingName.Tolerated));
            SetMutualRelationship(FactionName.Dogs, FactionName.Snakes,
                new(StandingName.Tolerated));
            SetMutualRelationship(FactionName.Dogs, FactionName.Boars,
                new(StandingName.Tolerated));
            SetMutualRelationship(FactionName.Snakes, FactionName.Boars,
                new(StandingName.Tolerated));

            SetMutualRelationship(FactionName.Players, FactionName.Rats,
                new(StandingName.Tolerated), new(StandingName.Tolerated));
            SetMutualRelationship(FactionName.Players, FactionName.Cats,
                new(StandingName.ApprovenOf));
            SetMutualRelationship(FactionName.Players, FactionName.Dogs,
                new(StandingName.Liked));
            SetMutualRelationship(FactionName.Players, FactionName.Snakes,
                new(StandingName.DisapprovenOf));
            SetMutualRelationship(FactionName.Players, FactionName.Boars,
                new(StandingName.Disliked));
            SetMutualRelationship(FactionName.Players, FactionName.BeastGod,
                new(StandingName.Hated));
        }

        public void SetMutualRelationship(FactionName a, FactionName b, Relationship aTowardsB, Relationship? bTowardsA = null)
        {
            GetRelationships(a).Set(b, aTowardsB);
            GetRelationships(b).Set(a, bTowardsA ?? aTowardsB);
        }

        public void UpdateRelationship(FactionName a, FactionName b, Func<Relationship, Relationship> update, out Relationship value)
        {
            GetRelationships(a).Update(b, update, out value);
        }
        public FactionSystem(EventBus bus, GameEntities entities)
            : base(bus)
        {
            Entities = entities;
            Relationships = new Dictionary<FactionName, FactionRelationships>();
            foreach (var val in Enum.GetValues<FactionName>()) {
                Relationships[val] = new FactionRelationships(val);
            }
        }
    }
}
