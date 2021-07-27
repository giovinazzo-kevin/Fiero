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
    [SingletonDependency]
    public class GameGlossaries
    {
        protected readonly Dictionary<FactionName, FactionGlossary> FactionGlossaries;

        protected GameLocalizations<LocaleName> Localizations;

        public GameGlossaries(GameLocalizations<LocaleName> localizations)
        {
            FactionGlossaries = new Dictionary<FactionName, FactionGlossary>();
            Localizations = localizations;
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
