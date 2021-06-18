using System.Linq;
using System.Numerics;

namespace Fiero.Business
{
    public readonly struct ConflictResolutionContext
    {
        public readonly Actor[] A;
        public readonly int[] AIds;
        public readonly Actor[] B;
        public readonly int[] BIds;
        public readonly Relationship ARel;
        public readonly Vector3 ARelVec;
        public readonly Relationship BRel;
        public readonly Vector3 BRelVec;
        public readonly Personality APers;
        public readonly Vector3 APersVec;
        public readonly Personality BPers;
        public readonly Vector3 BPersVec;
        
        public ConflictResolutionContext(Actor[] a, Actor[] b, Vector3 aRelVec, Vector3 bRelVec, Vector3 aPersVec, Vector3 bPersVec)
        {
            A = a;
            AIds = a.Select(a => a.Id).ToArray();
            B = b;
            BIds = b.Select(b => b.Id).ToArray();
            ARel = Relationship.FromVector(aRelVec);
            ARelVec = aRelVec;
            BRel = Relationship.FromVector(bRelVec);
            BRelVec = bRelVec;
            APers = Personality.FromVector(aPersVec);
            APersVec = aPersVec;
            BPers = Personality.FromVector(bPersVec);
            BPersVec = bPersVec;
        }
    }
}
