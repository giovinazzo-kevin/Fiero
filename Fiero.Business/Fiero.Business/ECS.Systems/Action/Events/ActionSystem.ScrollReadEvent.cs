namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly record struct ScrollReadEvent(Actor Actor, Scroll Scroll);
    }
}
