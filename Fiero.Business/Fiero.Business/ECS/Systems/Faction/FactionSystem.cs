using Fiero.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    public class FactionSystem
    {
        public readonly GameEntities Entities;
        protected readonly Dictionary<FactionName, FactionRelationships> Relationships;
        public readonly List<IConflictResolver> ConflictResolvers;

        public FactionRelationships GetRelationships(FactionName faction) => Relationships[faction];
        public void SetMutualRelationship(FactionName a, FactionName b, Relationship aTowardsB, Relationship? bTowardsA = null)
        {
            GetRelationships(a).Set(b, aTowardsB);
            GetRelationships(b).Set(a, bTowardsA ?? aTowardsB);
        }

        public bool TryUpdateRelationship(FactionName a, FactionName b, Func<Relationship, Relationship> update, out Relationship value)
        {
            return GetRelationships(a).TryUpdate(b, update, out value);
        }

        public bool TryCreateConflict(
            FactionName a, Func<Actor, int, bool> aSelect, 
            FactionName b, Func<Actor, int, bool> bSelect,
            out Conflict conflict,
            Func<IEnumerable<Conflict>, IOrderedEnumerable<Conflict>> orderCandidates = null)
        {
            orderCandidates ??= (x => x.OrderBy(y => 0));

            var rng = new Random();

            var aMembers = Entities.GetComponents<FactionComponent>()
                .Where(c => c.Type == a).Select(c => Entities.GetProxy<Actor>(c.EntityId));
            var bMembers = Entities.GetComponents<FactionComponent>()
                .Where(c => c.Type == b).Select(c => Entities.GetProxy<Actor>(c.EntityId));
            var aGroup = aMembers.Where(aSelect).ToArray();
            var bGroup = bMembers.Where(bSelect).ToArray();

            var aPowerPerception = aGroup.Average(a => bGroup.Select(b => (int)a.PowerComparison(b) / 3f)
                .OrderBy(x => Math.Abs(x)).First());
            var bPowerPerception = bGroup.Average(b => aGroup.Select(a => (int)b.PowerComparison(a) / 3f)
                .OrderBy(x => Math.Abs(x)).First());
            var aPowerName = (PowerComparisonName)(int)Math.Round(aPowerPerception * 3);
            var bPowerName = (PowerComparisonName)(int)Math.Round(bPowerPerception * 3);

            // Since both factions have an "opinion" that represents how strong they feel compared to each other,
            // the answer lies in the difference between their opinions: if both think they're at an advantage,
            // it means it's actually a fair fight. If one thinks it's dominating the other, and the other thinks
            // it's being dominated, then it's clearly a one-sided fight.
            // The division by two is there to keep the numbers in the [-3,3] range.

            // As for standing and trust, the difference is still a measure of asymmetry but the reading is less specific.
            // For example, if A loves B but B hates A, this is going to show up as diffRel having a standing of "Loved";
            // and the same goes for trust as well. This can be exploited to generate motives for new conflicts.

            var aRel = GetRelationships(a).Get(b).With(aPowerName);
            var bRel = GetRelationships(b).Get(a).With(bPowerName);
            var aRelVec = aRel.ToVector();
            var bRelVec = bRel.ToVector();

            // Individual actors also have their own personality, and that should be factored in as well.
            // The "group personality" is considered first as that determines how the group acts on average.
            // The difference in group personalities is similar to dRel above, as it highlights the asymmetries in
            // group thought, and this can also be exploited to generate conflicts.

            var aPersVec = aGroup.Select(a => a.Properties.Personality.ToVector()).Aggregate((a, b) => a + b) / aGroup.Length;
            var bPersVec = bGroup.Select(b => b.Properties.Personality.ToVector()).Aggregate((a, b) => a + b) / bGroup.Length;

            var resolutionContext = new ConflictResolutionContext(aGroup, bGroup, aRelVec, bRelVec, aPersVec, bPersVec);

            var candidates = orderCandidates(ConflictResolvers
                .TrySelect(r => (r.TryResolve(resolutionContext, out var conflict), conflict)))
                .ToArray();

            conflict = candidates.FirstOrDefault();
            return candidates.Any();
        }

        public bool TryEnactConflict(Conflict conflict)
        {
            // TODO: handle passive dialogues, scheduling of events, etc.
            return true;
        }

        public FactionSystem(GameEntities entities, IEnumerable<IConflictResolver> conflictResolvers)
        {
            Entities = entities;
            Relationships = new Dictionary<FactionName, FactionRelationships>();
            foreach (var val in Enum.GetValues<FactionName>()) {
                Relationships[val] = new FactionRelationships(val);
            }
            ConflictResolvers = new(conflictResolvers);
        }
    }
}
