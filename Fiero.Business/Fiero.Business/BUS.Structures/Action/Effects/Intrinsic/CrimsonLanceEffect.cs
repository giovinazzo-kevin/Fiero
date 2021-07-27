using System.Linq;

namespace Fiero.Business
{
    /// <summary>
    /// Gather blood in a 3x3 around you and convert it into a powerful projectile
    /// </summary>
    public class CrimsonLanceEffect : UseEffect
    {
        public override string Name => "$Spell.CrimsonLance.Name$";
        public override string Description => "$Spell.CrimsonLance.Desc$";

        protected override void OnApplied(GameSystems systems, Entity owner, Actor target)
        {
            var splatterHere = systems.Floor.GetFeaturesAt(target.FloorId(), target.Position())
                .TrySelect(f => (f.TryCast<BloodSplatter>(out var splatter), splatter))
                .SingleOrDefault();
            if (splatterHere is null || !splatterHere.Blood.TryRemove(10))
                return;
            if (splatterHere.Blood.Amount == 0) {
                systems.Floor.RemoveFeature(splatterHere);
            }
            target.Log?.Write("$Spell.Bloodbath.ProcMessage$");
        }
    }
}
