using Fiero.Core;
using System;

namespace Fiero.Business
{
    public partial class ActionSystem : EcsSystem
    {
        private bool HandleQuaffPotion(ActorTime t, ref IAction action, ref int? cost)
        {
            if (action is QuaffPotionAction a)
            {
                return ItemConsumed.Handle(new(t.Actor, a.Potion))
                    && PotionQuaffed.Handle(new(t.Actor, a.Potion));
            }
            else throw new NotSupportedException(action.GetType().Name);
        }
    }
}
