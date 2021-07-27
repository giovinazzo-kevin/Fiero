using System;

namespace Fiero.Business
{
    public class DialogueTriggeredEventArgs : EventArgs
    {
        public readonly DialogueNode Sender;
        public readonly DrawableEntity DialogueStarter;
        public readonly DrawableEntity[] DialogueListeners;


        public DialogueTriggeredEventArgs(DialogueNode sender, DrawableEntity dialogueStarter, params DrawableEntity[] dialogueListeners)
        {
            Sender = sender;
            DialogueStarter = dialogueStarter;
            DialogueListeners = dialogueListeners;
        }
    }
}
