using System.Text;

namespace Fiero.Business
{
    public class EffectsComponent : EcsComponent
    {
        public bool Lock { get; set; }

        public readonly HashSet<EffectDef> Intrinsic = new();
        public readonly HashSet<Effect> Active = new();

        public string Description { get; set; }

        public string Describe(EffectDef def, Effect modifier)
        {
            var sb = new StringBuilder();
            var modifierFx = modifier switch
            {
                GrantedOnEquip => "when equipped,",
                GrantedOnQuaff => "when quaffed,",
                GrantedOnRead => "when read,",
                GrantedOnUse => "when used,",
                GrantedWhenHitByWeapon => "when striking,",
                GrantedWhenHitByThrownItem => "when thrown,",
                GrantedWhenHitByZappedWand => "when zapped,",
                GrantedWhenSteppedOn => "when stepped on,",
                GrantedWhenTargetedByScroll { Modifier: ScrollModifierName.AreaAffectsEnemies } => "when read, targets all enemies and",
                GrantedWhenTargetedByScroll { Modifier: ScrollModifierName.AreaAffectsAllies } => "when read, targets all allies and",
                GrantedWhenTargetedByScroll { Modifier: ScrollModifierName.AreaAffectsItems } => "when read, targets all items and",
                GrantedWhenTargetedByScroll { Modifier: ScrollModifierName.AreaAffectsEveryone } => "when read, targets everyone",
                GrantedWhenTargetedByScroll { Modifier: ScrollModifierName.AreaAffectsEveryoneButTarget } => "when read, targets everyone except the user",
                GrantedWhenTargetedByScroll { Modifier: ScrollModifierName.Self } => "when read, targets the user",
                _ => ""
            };
            var targetFx = $" applies {def.Name switch
            {
                EffectName.Script => def.Script.Name.ToUpperInvariant(),
                var name => name.ToString().ToUpperInvariant()
            }}";
            sb.Append(modifierFx);
            sb.Append(targetFx);
            if (!string.IsNullOrEmpty(def.Arguments))
                sb.Append($"({def.Arguments})");
            sb.Append(Duration(def));
            sb.Append(Chance(def));
            return sb.ToString();

            string Duration(EffectDef def)
            {
                if (def.Duration is null)
                    return "";
                else return $" for {def.Duration} turns";
            }

            string Chance(EffectDef def)
            {
                if (def.Chance is null)
                    return "";
                else return $" with {def.Chance * 100:0.00}% probability";
            }
        }
    }
}
