using Fiero.Core;

namespace Fiero.Business
{
    public class PotionComponent : EcsComponent
    {
        public EffectDef QuaffEffect { get; set; }
        public EffectDef ThrowEffect { get; set; }
    }
}
