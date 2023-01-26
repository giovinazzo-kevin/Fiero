using System.Collections.Generic;
using System.Linq;
using Unconcern.Common;

namespace Fiero.Business
{
    public class NonStacking : ModifierEffect
    {
        public NonStacking(EffectDef source) : base(source)
        {
        }

        public override string DisplayName => $"$Effect.{Source.Name}$";
        public override string DisplayDescription => $"$Effect.NonStacking$";
        public override EffectName Name => Source.Name;

        protected override void OnStarted(GameSystems systems, Entity owner)
        {
            if (owner.Effects == null || !owner.Effects.Active.Any(e => e.Name switch
            {
                EffectName.Script when e is ScriptEffect se => se.Script?.ScriptProperties.ScriptPath == Source.Script?.ScriptProperties.ScriptPath,
                _ => e.Name == Source.Name
            }))
            {
                Source.Resolve(null).Start(systems, owner);
            }
        }

        protected override IEnumerable<Subscription> RouteEvents(GameSystems systems, Entity owner)
        {
            yield break;
        }
    }
}
