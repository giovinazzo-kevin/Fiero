namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly struct ActorGainedEffectEvent
        {
            public readonly Actor Actor;
            public readonly Effect Effect;
            public ActorGainedEffectEvent(Actor actor, Effect effect)
                => (Actor, Effect) = (actor, effect);
        }
    }
}
