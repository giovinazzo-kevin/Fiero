using Fiero.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Utf8Json;

namespace Fiero.Business
{
    public class GameGlossaries
    {
        protected readonly Dictionary<FactionName, FactionGlossary> FactionGlossaries;

        protected GameLocalizations<LocaleName> Localizations;

        public GameGlossaries(GameLocalizations<LocaleName> localizations)
        {
            FactionGlossaries = new Dictionary<FactionName, FactionGlossary>();
            Localizations = localizations;
        }

        public string GetMonsterName(FactionName faction, MonsterTierName tier)
        {
            if(!FactionGlossaries.TryGetValue(faction, out var glossary)
            || !glossary.TryGetName(tier, out var name)) {
                return faction switch {
                    FactionName.Rats => tier switch {
                        MonsterTierName.Two => "black rat",     // Flank role (fast)
                        MonsterTierName.Three => "plague rat",  // Debuff role (causes poisoning)
                        MonsterTierName.Four => "woolly rat",   // Tank role
                        MonsterTierName.Five => "big rat",      // Leader role (empowers nearby rats, only one big rat at a time, highest tier rat replaces leader on death)
                        _ => "brown rat"
                    },
                    FactionName.Snakes => tier switch {
                        MonsterTierName.Two => "twig snake",    // Debuff role (causes bleeding)
                        MonsterTierName.Three => "ball python", // Tank/Debuff role (constricts)
                        MonsterTierName.Four => "death adder",  // Debuff role (causes paralysis)
                        MonsterTierName.Five => "king cobra",   // DPS role (causes neurovenom, which deals intense pain and stacks quickly)
                        _ => "bull snake"
                    },
                    FactionName.Dogs => tier switch {
                        MonsterTierName.Two => "black rat",     // Flank role
                        MonsterTierName.Three => "plague rat",  // Debuff role
                        MonsterTierName.Four => "woolly rat",   // Tank role
                        MonsterTierName.Five => "big rat",      // Leader role
                        _ => "slime"
                    },
                    _ => "???"
                };
            }
            return name;
        }

        public void LoadFactionGlossary(FactionName faction)
        {
            var names = new FactionNames(
                Localizations.GetArray($"Faction.{faction}.MonsterNames[0]"),
                Localizations.GetArray($"Faction.{faction}.MonsterNames[1]"),
                Localizations.GetArray($"Faction.{faction}.MonsterNames[2]"),
                Localizations.GetArray($"Faction.{faction}.MonsterNames[3]"),
                Localizations.GetArray($"Faction.{faction}.MonsterNames[4]")
            );
            var glossary = new FactionGlossary(names);
            FactionGlossaries[faction] = glossary;
        }

    }
}
