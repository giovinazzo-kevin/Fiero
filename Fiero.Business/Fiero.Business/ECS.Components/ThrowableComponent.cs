using Fiero.Core;

namespace Fiero.Business
{
    public class ThrowableComponent : EcsComponent
    {
        public ThrowableName Name { get; set; }
        public int BaseDamage { get; set; } = 1;
        public int MaximumRange { get; set; } = 3;
        public float MulchChance { get; set; } = 0.25f;
        public bool ThrowsUseCharges { get; set; }
        public ThrowName Throw { get; set; }
    }
}
