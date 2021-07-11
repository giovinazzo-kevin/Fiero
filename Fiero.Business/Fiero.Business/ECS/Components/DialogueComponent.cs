using Fiero.Core;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Fiero.Business
{

    public class DialogueComponent : EcsComponent
    {
        public HashSet<IDialogueTrigger> Triggers { get; set; } = new HashSet<IDialogueTrigger>();
    }
}
