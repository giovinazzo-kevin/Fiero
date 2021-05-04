using Fiero.Core;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Fiero.Business
{

    public class DialogueComponent : Component
    {
        public HashSet<IDialogueTrigger> Triggers { get; set; } = new HashSet<IDialogueTrigger>();

        public DialogueNode Saying { get; set; }
        public DialogueNode Listening { get; set; }
    }
}
