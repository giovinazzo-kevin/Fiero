using Fiero.Core;
using System;
using System.Collections.Generic;

namespace Fiero.Business
{
    public sealed class PointTargetingShape : TargetingShape
    {
        public readonly int MaxRange;
        public Coord Offset { get; private set; }


        public PointTargetingShape(Coord origin, int maxRange = 0) : base(origin)
        {
            MaxRange = maxRange;
        }

        public override IEnumerable<Coord> GetPoints()
        {
            yield return Origin + Offset;
        }

        public override bool TryOffset(Coord offs)
        {
            var newOffset = Offset + offs;
            if((Origin + newOffset).DistTaxi(Origin) > MaxRange) {
                return false;
            }
            Offset = newOffset;
            return true;
        }

        public override bool CanRotateWithDirectionKeys() => false;

        public override bool TryRotateCCw()
        {
            return false;
        }

        public override bool TryRotateCw()
        {
            return false;
        }

        public override bool TryAutoTarget(Func<Coord, bool> validTarget, Func<Coord, bool> obstacle)
        {
            foreach (var p in Shapes.Box(Origin, MaxRange)) {
                if(obstacle(p)) {
                    continue;
                }
                if(validTarget(p)) {
                    Offset = p - Origin;
                    return true;
                }
            }
            return false;
        }
    }
}
