using System.Collections.Generic;

namespace Fiero.Business
{
    public interface IDialogueTrigger
    {
        string DialogueNode { get; }
        bool Repeatable { get; }
        bool TryTrigger(Floor floor, Drawable speaker, out IEnumerable<Drawable> listeners);
        void OnTrigger();
    }
}
