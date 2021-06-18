using Fiero.Core;
using System;
using System.Linq;
using System.Numerics;

namespace Fiero.Business
{
    public class AssaultConflictResolver : IConflictResolver
    {
        public bool TryResolve(ConflictResolutionContext ctx, out Conflict conflict)
        {
            conflict = default;
            var standingAverage = (ctx.ARelVec.X + ctx.BRelVec.X) / 2f; // -1 = hate, 0 = neutral, 1 = love
            var powerDifference = (ctx.ARelVec.Z - ctx.BRelVec.Z) / 2f; // -1 = a loses, 0 = fair fight, 1 = a wins
            var averageImpulsivity = (-(ctx.APersVec.Z + ctx.APersVec.Z) / 2f + 1) / 2f;
            var averageSolitude = (-(ctx.APersVec.Y + ctx.APersVec.Y) / 2f + 1) / 2f;
            var averageEgo = (-(ctx.APersVec.X + ctx.APersVec.X) / 2f + 1) / 2f;
            // Motives for assault include: not liking each other, being physically superior or in greater numbers.
            if ((powerDifference > 0.33f || ctx.A.Length >= 2 * ctx.B.Length) && standingAverage < -0.33f) {
                // The chance for this motive being chosen depends on how impulsive and selfish both groups are
                if (new GaussianNumber(averageEgo, (1 - averageImpulsivity) * 4).CoinFlip()) {
                    // The culpable parties are those with a negative standing.
                    conflict = new(
                        ConflictName.Assault,
                        ConflictMotiveName.Dominance,
                        new(nameof(ctx.A), ctx.ARelVec.X < 0, ctx.AIds),
                        new(nameof(ctx.B), ctx.BRelVec.X < 0, ctx.BIds));
                    return true;
                }
            }
            // Other motives include personal grudges among individual members that can be raised on behalf of the whole group.
            var relationships = ctx.A.Select(a => (a, ctx.B.TrySelect(b => (a.Properties.Relationships.TryGet(b, out var s), s))))
                .Concat(ctx.B.Select(b => (b, ctx.A.TrySelect(a => (b.Properties.Relationships.TryGet(a, out var s), s)))))
                .SelectMany(t => t.Item2.Select(x => (Actor: t.Item1, Rel: x)));
            if (relationships.FirstOrDefault(r => r.Rel.Standing == StandingName.Hated && r.Rel.Trust < TrustName.Known) 
                is { } hateMotive && hateMotive.Actor != null) /* I hate you and have no reason to trust you */ {
                // The chance for this motive being chosen depends on the instigator's ego and the group's gregariousness
                var ego = hateMotive.Actor.Properties.Personality.ToVector().X;
                if (new GaussianNumber(ego, averageSolitude * 4).CoinFlip()) {
                    conflict = new(
                        ConflictName.Assault,
                        ConflictMotiveName.Hate,
                        new(nameof(ctx.A), ctx.A.Contains(hateMotive.Actor), ctx.AIds),
                        new(nameof(ctx.B), ctx.B.Contains(hateMotive.Actor), ctx.BIds));
                    return true;
                }
            }
            if (relationships.FirstOrDefault(r => r.Rel.Trust == TrustName.Feared && r.Rel.Standing < StandingName.Tolerated) 
                is { } trustMotive && trustMotive.Actor != null) /* I don't trust you and have no reason to like you */ {
                // The chance for this motive being chosen depends on the instigator's gregariousness and the group's egotism
                var greg = hateMotive.Actor.Properties.Personality.ToVector().Y;
                if (new GaussianNumber(1 - greg, (1 - averageEgo) * 4).CoinFlip()) {
                    conflict = new(
                        ConflictName.Assault,
                        ConflictMotiveName.Fear,
                        new(nameof(ctx.A), ctx.A.Contains(trustMotive.Actor), ctx.AIds),
                        new(nameof(ctx.B), ctx.B.Contains(trustMotive.Actor), ctx.BIds));
                    return true;
                }
            }
            return false;
        }
    }
}
