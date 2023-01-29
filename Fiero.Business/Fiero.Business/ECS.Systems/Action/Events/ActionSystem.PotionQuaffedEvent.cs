namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly record struct PotionQuaffedEvent(Actor Actor, Potion Potion);
    }
}
