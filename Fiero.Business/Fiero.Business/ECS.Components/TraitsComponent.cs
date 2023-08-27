using Fiero.Core;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    public class TraitsComponent : EcsComponent
    {
        private readonly HashSet<Trait> Intrinsic = new();
        private readonly HashSet<Trait> Extrinsic = new();

        public IEnumerable<Trait> Active => Intrinsic
            .Where(i => !Extrinsic.Contains(i)).Concat(Extrinsic);

        public bool HasTrait(Trait trait)
        {
            return Intrinsic.Contains(trait) || Extrinsic.Contains(trait);
        }

        public void AddIntrinsicTrait(Trait trait)
        {
            Intrinsic.Add(trait);
        }

        public void RemoveIntrinsicTrait(Trait trait)
        {
            Intrinsic.RemoveWhere(x => x.Name == trait.Name);
        }

        public void AddExtrinsicTrait(Trait trait)
        {
            Extrinsic.Add(trait);
        }

        public void RemoveExtrinsicTrait(Trait trait)
        {
            Extrinsic.RemoveWhere(x => x.Name == trait.Name);
        }
    }
}
