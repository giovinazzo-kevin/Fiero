using Ergo.Lang;

namespace Fiero.Business
{
    [Term(Functor = "trigger", Marshalling = TermMarshalling.Named)]
    public interface IDialogueTrigger
    {
        string Node { get; }
        string FullPath { get; }
        bool Repeatable { get; }
        bool TryTrigger(FloorId floor, PhysicalEntity speaker, out IEnumerable<DrawableEntity> listeners);
        void OnTrigger();
    }
}
