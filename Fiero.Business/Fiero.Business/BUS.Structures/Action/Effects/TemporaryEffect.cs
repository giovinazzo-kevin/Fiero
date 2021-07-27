using System;
using System.Collections.Generic;
using Unconcern.Common;

namespace Fiero.Business
{
    public class TemporaryEffect : Effect
    {
        public readonly Effect Source;
        public readonly int Duration;
        public int Time { get; private set; }
        
        public TemporaryEffect(Effect source, int duration)
        {
            Source = source;
            Duration = duration;
        }

        protected override void OnStarted(GameSystems systems, Entity owner)
        {
            Source.Start(systems, owner);
        }

        protected override void OnEnded()
        {
            try {
                Source.End();
            }
            catch(ObjectDisposedException) { }
        }

        protected override IEnumerable<Subscription> RouteEvents(GameSystems systems, Entity owner)
        {
            yield return systems.Action.TurnEnded.SubscribeHandler(e => {
                if (++Time >= Duration) End();
            });
        }
    }
}
