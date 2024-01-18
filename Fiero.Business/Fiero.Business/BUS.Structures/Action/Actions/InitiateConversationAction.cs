namespace Fiero.Business
{
    public readonly struct InitiateConversationAction : IAction
    {
        public readonly Actor Speaker;
        public InitiateConversationAction(Actor npc)
        {
            Speaker = npc;
        }
        ActionName IAction.Name => ActionName.Interact;
        int? IAction.Cost => 0;
    }
}
