using Ergo.Lang;

namespace Fiero.Core
{
    [TransientDependency]
    [Term(Marshalling = TermMarshalling.Named)]
    public abstract class EcsComponent
    {
        public int Id { get; internal set; }
        public int EntityId { get; internal set; }
    }
}
