using Ergo.Lang;
using Fiero.Core;

namespace Fiero.Business
{
    public class Entity : EcsEntity
    {
        [NonTerm]
        [RequiredComponent]
        public InfoComponent Info { get; private set; }
        [NonTerm]
        public EffectsComponent Effects { get; private set; }
        public override string ToString() => Info?.Name;
    }
}
