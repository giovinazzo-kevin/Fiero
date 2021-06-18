using Fiero.Core;

namespace Fiero.Business
{
    public readonly struct ConflictResolution
    {
        public readonly string Name;
        public readonly Conflict Conflict;
        public readonly Knob<float> Split;

        public ConflictResolution(string name, Conflict c, float split)
        {
            Name = name;
            Conflict = c;
            Split = new(-1, 1, split);
        }
    }
}
