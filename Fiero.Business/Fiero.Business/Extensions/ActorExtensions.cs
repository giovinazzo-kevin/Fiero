using Fiero.Core;
using System.Drawing;
using System.Net.Http.Headers;

namespace Fiero.Business
{

    public static class ActorExtensions
    {
        public static bool IsHotileTowards(this Actor a, Actor b)
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

        public static PowerComparisonName PowerComparison(this Actor a, Actor b)
        {
            if (a.Npc?.IsBoss ?? false) {
                return PowerComparisonName.Dominating;
            }
            if (b.Npc?.IsBoss ?? false) {
                return PowerComparisonName.Dominated;
            }
            // If everything else fail, default to faction-based rules.
            // Note that these are "subjective", meaning that they are not necessarily symmetric.
            return a.Faction.Type switch {
                // Rats feel predictably weak in combat (which is why they're so resourceful)
                FactionName.Rats => b.Faction.Type switch {
                    FactionName.Snakes => PowerComparisonName.AtDisadvantage,
                    FactionName.Cats => PowerComparisonName.Overwhelmed,
                    FactionName.Dogs => PowerComparisonName.Dominated,
                    FactionName.Boars => PowerComparisonName.Dominated,
                    _ => PowerComparisonName.Dominated
                },
                // Snakes know they're weak too, but they're more confident against enemies of their size
                FactionName.Snakes => b.Faction.Type switch {
                    FactionName.Rats => PowerComparisonName.Overwhelming,
                    FactionName.Cats => PowerComparisonName.AtAdvantage,
                    FactionName.Dogs => PowerComparisonName.Dominated,
                    FactionName.Boars => PowerComparisonName.Dominated,
                    _ => PowerComparisonName.FairFight
                },
                // Cats are very agile and confident, so they always feel like taking big enemies head on
                FactionName.Cats => b.Faction.Type switch {
                    FactionName.Rats => PowerComparisonName.Dominating,
                    FactionName.Snakes => PowerComparisonName.AtAdvantage,
                    FactionName.Dogs => PowerComparisonName.FairFight,
                    FactionName.Boars => PowerComparisonName.AtDisadvantage,
                    _ => PowerComparisonName.AtAdvantage
                },
                // Dogs are strong and coordinated but not as reckless as cats
                FactionName.Dogs => b.Faction.Type switch {
                    FactionName.Rats => PowerComparisonName.Dominating,
                    FactionName.Snakes => PowerComparisonName.Overwhelming,
                    FactionName.Cats => PowerComparisonName.FairFight,
                    FactionName.Boars => PowerComparisonName.AtDisadvantage,
                    _ => PowerComparisonName.AtAdvantage
                },
                // Boars simply don't give a fuck
                FactionName.Boars => b.Faction.Type switch {
                    FactionName.Rats => PowerComparisonName.Dominating,
                    FactionName.Snakes => PowerComparisonName.Dominating,
                    FactionName.Cats => PowerComparisonName.Overwhelming,
                    FactionName.Dogs => PowerComparisonName.Overwhelming,
                    _ => PowerComparisonName.Overwhelming
                },
                _ => PowerComparisonName.FairFight
            };
        }

        public static FloorId FloorId(this Actor a) => a.ActorProperties.FloorId;

        public static bool CanSee(this FloorSystem floorSystem, Actor a, Drawable other) 
            => floorSystem.CanSee(a, other.Physics.Position);
        public static bool CanSee(this FloorSystem floorSystem, Actor a, Coord bPos)
        {
            var aPos = a.Physics.Position;
            return Utils.Bresenham(new(aPos.X, aPos.Y), new(bPos.X, bPos.Y), p => {
                if(!floorSystem.TryGetTileAt(a.FloorId(), p, out var tile)) {
                    return false;
                }
                if (tile.TileProperties.BlocksMovement) {
                    return false;
                }
                return true;
            });
        }
    }
}
