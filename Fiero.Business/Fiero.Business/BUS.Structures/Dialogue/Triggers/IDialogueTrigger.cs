using System.Collections.Generic;

namespace Fiero.Business
{
    public interface IDialogueTrigger
    {
        string DialogueNode { get; }
        bool Repeatable { get; }
        bool TryTrigger(FloorId floor, PhysicalEntity speaker, out IEnumerable<DrawableEntity> listeners);
        void OnTrigger();
    }
}
