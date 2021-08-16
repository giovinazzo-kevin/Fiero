using Fiero.Core;

namespace Fiero.Business
{
    public class TrapEffect : GrantedWhenSteppedOn
    {
        public TrapEffect() : base(Rng.Random.Choose(new EffectDef[] { 
            new(EffectName.Confusion, duration: Rng.Random.Between(5, 15)),
            new(EffectName.Sleep, duration: Rng.Random.Between(5, 15)),
            new(EffectName.UncontrolledTeleport),
        }), true, true)
        {
        }
    }
}
