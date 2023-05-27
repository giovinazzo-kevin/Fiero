using Ergo.Lang;
using Fiero.Core;

namespace Fiero.Business
{
    [Term(Marshalling = TermMarshalling.Named)]
    public class Entity : EcsEntity
    {
        [RequiredComponent]
        public InfoComponent Info { get; private set; }
        [NonTerm]
        public EffectsComponent Effects { get; private set; }
        public override string ToString() => Info?.Name;
    }
}
