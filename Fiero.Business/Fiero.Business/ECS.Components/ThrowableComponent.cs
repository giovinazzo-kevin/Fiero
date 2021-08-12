using Fiero.Core;

namespace Fiero.Business
{
    public class ThrowableComponent : EcsComponent
    {
        public int BaseDamage { get; set; } = 1;
        public int MaximumRange { get; set; } = 3;
        public ThrowName Throw { get; set; }
    }
}
