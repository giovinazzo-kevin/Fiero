using System.Collections.Generic;
using Unconcern.Common;

namespace Fiero.Business
{

    /// <summary>
    /// Cast effects can be applied to:
    /// - Spells:
    ///     - The effect is applied to the actor that casts the spell, and it ends when the the spell is forgotten.
    /// </summary>
    public abstract class CastEffect : ModifierEffect
    {
        protected CastEffect(EffectDef source) : base(source)
        {
        }

        public abstract bool ShouldApply(GameSystems systems, Entity owner, Actor caster);
        protected abstract bool OnApplied(GameSystems systems, Entity owner, Actor caster, PhysicalEntity[] targets);
        protected override IEnumerable<Subscription> RouteEvents(GameSystems systems, Entity owner)
        {
            if (owner.TryCast<Spell>(out var spell))
            {
                yield return systems.Action.SpellLearned.SubscribeHandler(e =>
                {
                    if (e.Spell == spell)
                    {
                        Subscriptions.Add(systems.Action.SpellTargeted.SubscribeResponse(e =>
                        {
                            if (e.Spell == spell)
                            {
                                return ShouldApply(systems, owner, e.Actor);
                            }
                            return true;
                        }));
                        Subscriptions.Add(systems.Action.SpellCast.SubscribeResponse(e =>
                        {
                            if (e.Spell == spell)
                            {
                                return OnApplied(systems, owner, e.Actor, e.Targets);
                            }
                            return true;
                        }));
                    }
                });
                yield return systems.Action.SpellForgotten.SubscribeHandler(e =>
                {
                    if (e.Spell == spell)
                    {
                        End(); // TODO: If spells become singletons, remove this
                    }
                });
            }
        }
    }
}
