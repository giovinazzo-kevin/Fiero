using Fiero.Core;
using System.Collections.Generic;

namespace Fiero.Business
{
    public class SpellLibraryComponent : EcsComponent
    {
        private readonly HashSet<Spell> _knownSpells = new();

        public IEnumerable<Spell> KnownSpells => _knownSpells;

        public bool Learn(Spell spell)
        {
            if (_knownSpells.Contains(spell))
                return false;
            _knownSpells.Add(spell);
            return true;
        }

        public bool Forget(Spell spell)
        {
            if (!_knownSpells.Contains(spell))
                return false;
            _knownSpells.Remove(spell);
            return true;
        }
    }
}
