using Fiero.Core;

namespace Fiero.Business
{
    public readonly struct FactionGlossary
    {
        public readonly FactionNames Names;

        public bool TryGetName(MonsterTierName tier, out string name)
        {
            name = default;
            var array = tier switch {
                MonsterTierName.Two => Names.Tier2,
                MonsterTierName.Three => Names.Tier3,
                MonsterTierName.Four => Names.Tier4,
                MonsterTierName.Five => Names.Tier5,
                _ => Names.Tier1
            };

            if(array.Length == 0) {
                return false;
            }
            name = array[Rng.Random.Next(array.Length)];
            return true;
        }

        public FactionGlossary(FactionNames names)
        {
            Names = names;
        }
    }
}
