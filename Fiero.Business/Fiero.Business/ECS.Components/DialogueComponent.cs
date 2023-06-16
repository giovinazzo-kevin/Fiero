using Fiero.Core;
using System.Collections.Generic;

namespace Fiero.Business
{

    public class DialogueComponent : EcsComponent
    {
        public HashSet<IDialogueTrigger> Triggers { get; set; } = new HashSet<IDialogueTrigger>();
    }
}
