using Ergo.Lang;

namespace Fiero.Business
{
    public class TraitsComponent : EcsComponent
    {
        private readonly HashSet<Trait> Intrinsic = new();
        private readonly HashSet<Trait> Extrinsic = new();

        [NonTerm]
        public IEnumerable<Trait> Active => Intrinsic
            .Where(i => !Extrinsic.Contains(i)).Concat(Extrinsic);

        public bool HasTrait(Trait trait)
        {
            return Intrinsic.Contains(trait) || Extrinsic.Contains(trait);
        }

        public void AddIntrinsicTrait(Trait trait)
        {
            Intrinsic.Add(trait with { KillSwitch = () => { } });
        }

        public bool AddExtrinsicTrait(Trait trait, Action killSwitch, out Trait removed)
        {
            trait = trait with { KillSwitch = killSwitch };
            var ret = Extrinsic.TryGetValue(trait, out removed);
            Extrinsic.Add(trait);
            return ret;
        }

        public void RemoveExtrinsicTrait(Trait trait, bool fireKillSwitch = false)
        {
            if (fireKillSwitch && Extrinsic.TryGetValue(trait, out var same)
                && same.Name == trait.Name)
                same.KillSwitch();
            Extrinsic.RemoveWhere(x => x.Name == trait.Name);
        }
    }
}
