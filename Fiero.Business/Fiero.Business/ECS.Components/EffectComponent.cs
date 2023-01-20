using Fiero.Core;
using System.Collections.Generic;

namespace Fiero.Business
{
    public class EffectsComponent : EcsComponent
    {
        public readonly HashSet<EffectDef> Intrinsic = new();
        public readonly HashSet<Effect> Active = new();
    }
}
