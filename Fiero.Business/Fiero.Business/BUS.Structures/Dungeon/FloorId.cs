using Ergo.Lang;

namespace Fiero.Business
{
    [Term(Marshalling = TermMarshalling.Positional, Functor = "f")]
    public readonly record struct FloorId(DungeonBranchName Branch, int Depth)
    {
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(Branch);
            hash.Add(Depth);
            return hash.ToHashCode();
        }

        public override string ToString() => $"{Branch.ToString()[0]}{Depth}";
    }
}
