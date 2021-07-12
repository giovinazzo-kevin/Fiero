namespace Fiero.Business
{
    public abstract class ActionProvider
    {
        public abstract IAction GetIntent(Actor actor);
    }
}
