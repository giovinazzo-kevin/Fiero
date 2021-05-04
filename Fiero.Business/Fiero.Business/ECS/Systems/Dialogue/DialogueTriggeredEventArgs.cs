using System;

namespace Fiero.Business
{
    public class DialogueTriggeredEventArgs : EventArgs
    {
        public readonly DialogueNode Sender;
        public readonly Drawable DialogueStarter;
        public readonly Drawable[] DialogueListeners;


        public DialogueTriggeredEventArgs(DialogueNode sender, Drawable dialogueStarter, params Drawable[] dialogueListeners)
        {
            Sender = sender;
            DialogueStarter = dialogueStarter;
            DialogueListeners = dialogueListeners;
        }
    }
}
