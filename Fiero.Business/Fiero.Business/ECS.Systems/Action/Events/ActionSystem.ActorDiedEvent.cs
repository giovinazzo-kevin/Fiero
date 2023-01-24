namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly record struct ActorDiedEvent(Actor Actor);
    }
}
