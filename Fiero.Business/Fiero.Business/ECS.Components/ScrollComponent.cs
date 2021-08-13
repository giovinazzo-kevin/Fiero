using Fiero.Core;

namespace Fiero.Business
{
    public class ScrollComponent : EcsComponent
    {
        public EffectDef Effect { get; set; }
        public ScrollModifierName Modifier { get; set; }
    }
}
