using Fiero.Core;

namespace Fiero.Business
{
    public class TrapEffect : GrantedWhenSteppedOn
    {
        public TrapEffect() : base(Rng.Random.Choose(new EffectDef[] { 
            new(EffectName.Confusion, duration: 10)
        }), true, true)
        {
        }
    }
}
