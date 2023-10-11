using Ergo.Lang;

namespace Fiero.Business
{
    [Term(Functor = "l", Marshalling = TermMarshalling.Positional)]
    public readonly record struct Location(FloorId FloorId, Coord Position);
}
