using Unconcern.Common;

namespace Fiero.Business
{
    /// <summary>
    /// Use effects can be applied to:
    /// - Throwables:
    ///     - The effect is applied to the actor that's hit by the thrown item.
    /// </summary>
    public abstract class ThrowEffect : ModifierEffect
    {
        protected ThrowEffect(EffectDef source) : base(source)
        {
        }

        protected abstract void OnApplied(MetaSystem systems, Entity owner, Actor source, Actor target);
        protected abstract void OnApplied(MetaSystem systems, Entity owner, Actor source, Coord location);

        protected override IEnumerable<Subscription> RouteEvents(MetaSystem systems, Entity owner)
        {
            if (owner.TryCast<Throwable>(out var throwable))
            {
                yield return systems.Get<ActionSystem>().ItemThrown.SubscribeHandler(e =>
                {
                    if (e.Item == owner)
                    {
                        if (e.Victim != null)
                        {
                            OnApplied(systems, owner, e.Actor, e.Victim);
                        }
                        else
                        {
                            OnApplied(systems, owner, e.Actor, e.Position);
                        }
                    }
                });
            }
        }
    }
}
