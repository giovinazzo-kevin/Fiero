using Fiero.Core;
using System;
using System.Linq;

namespace Fiero.Business
{
    public partial class ActionSystem : EcsSystem
    {
        private bool HandleCastSpell(ActorTime t, ref IAction action, ref int? cost)
        {
            if (action is CastSpellAction cast)
            {
                if (SpellTargeted.Handle(new(t.Actor, cast.Spell)))
                {
                    var systems = (GameSystems)_entities.ServiceFactory.GetInstance(typeof(GameSystems));
                    var validTargets = cast.TargetingShape.GetPoints()
                        .TrySelect(p => (_floorSystem.TryGetCellAt(t.Actor.FloorId(), p, out var cell), cell))
                        .SelectMany(c => c.GetDrawables())
                        .Where(d => cast.Spell.SpellProperties.TargetingFilter(systems, t.Actor, d));
                    return SpellCast.Handle(new(t.Actor, cast.Spell, validTargets.ToArray()));
                }
                return false;
            }
            else throw new NotSupportedException(action.GetType().Name);
        }
    }
}
