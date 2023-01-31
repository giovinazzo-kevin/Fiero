using Fiero.Core;
using Fiero.Core.Extensions;

namespace Fiero.Business
{
    public class TrapEffect : GrantedWhenSteppedOn
    {
        public TrapEffect() : base(Rng.Random.Choose(new EffectDef[] { 
            new(EffectName.Confusion, duration: Rng.Random.Between(3, 10)),
            new(EffectName.Sleep, duration: Rng.Random.Between(2, 6)),
            new(EffectName.UncontrolledTeleport)
        }), true, true)
        {
        }
    }
}
