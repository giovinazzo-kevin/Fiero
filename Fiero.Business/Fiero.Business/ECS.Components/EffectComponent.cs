using Fiero.Core;
using System.Collections.Generic;

namespace Fiero.Business
{
    public class EffectComponent : EcsComponent
    {
        public readonly HashSet<EffectDef> Intrinsic = new();
        public readonly HashSet<Effect> Active = new();
    }
}
