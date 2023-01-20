using Fiero.Core;

namespace Fiero.Business
{
    public class Entity : EcsEntity
    {
        [RequiredComponent]
        public InfoComponent Info { get; private set; }
        public EffectsComponent Effects { get; private set; }
        public override string ToString() => Info?.Name;
    }
}
