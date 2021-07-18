using System;

namespace Fiero.Business
{
    public readonly struct FloorId
    {
        public readonly DungeonBranchName Branch;
        public readonly int Depth;

        public FloorId(DungeonBranchName branch, int depth)
        {
            Branch = branch;
            Depth = depth;
        }

        public override bool Equals(object obj) => obj is FloorId other
            ? Branch == other.Branch && Depth == other.Depth
            : base.Equals(obj);

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(Branch);
            hash.Add(Depth);
            return hash.ToHashCode();
        }

        public static bool operator ==(FloorId left, FloorId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FloorId left, FloorId right)
        {
            return !(left == right);
        }
    }
}
