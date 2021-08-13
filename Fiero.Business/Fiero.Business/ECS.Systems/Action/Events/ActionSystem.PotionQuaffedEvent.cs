namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly struct PotionQuaffedEvent
        {
            public readonly Actor Actor;
            public readonly Potion Potion;
            public PotionQuaffedEvent(Actor actor, Potion potion)
                => (Actor, Potion) = (actor, potion);
        }
    }
}
