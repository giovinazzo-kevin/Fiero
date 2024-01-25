using Unconcern.Common;

namespace Fiero.Business
{
    public class Temporary : ModifierEffect
    {
        public readonly int Duration;
        public int Time { get; private set; }
        public override string DisplayName => $"$Effect.{Source.Name}$";
        public override string DisplayDescription => $"$Effect.Temporary$ ({Time})";
        public override EffectName Name => Source.Name;

        public Temporary(EffectDef source, int duration) : base(source)
        {
            Duration = duration;
        }

        protected override void OnStarted(MetaSystem systems, Entity owner, Entity source)
        {
            base.OnStarted(systems, owner, source);
            var effect = Source.Resolve(null);
            Ended += Temporary_Ended;
            effect.Start(systems, owner, source);

            void Temporary_Ended(Effect _)
            {
                effect.End(systems, owner);
                Ended -= Temporary_Ended;
            }
        }

        protected override IEnumerable<Subscription> RouteEvents(MetaSystem systems, Entity owner)
        {
            yield return systems.Get<ActionSystem>().TurnEnded.SubscribeHandler(e =>
            {
                if (Time++ >= Duration)
                {
                    End(systems, owner);
                }
            });
        }
    }
}
