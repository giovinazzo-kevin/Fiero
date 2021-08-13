namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly struct ActorLostEffectEvent
        {
            public readonly Actor Actor;
            public readonly Effect Effect;
            public ActorLostEffectEvent(Actor actor, Effect effect)
                => (Actor, Effect) = (actor, effect);
        }
    }
}
