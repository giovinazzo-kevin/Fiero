namespace Fiero.Business
{
    public readonly struct InitiateConversationAction : IAction
    {
        public readonly Actor NPC;
        public InitiateConversationAction(Actor npc)
        {
            NPC = npc;
        }
        ActionName IAction.Name => ActionName.Interact;
        int? IAction.Cost => 0;
    }
}
