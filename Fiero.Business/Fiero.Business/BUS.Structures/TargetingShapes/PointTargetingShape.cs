namespace Fiero.Business
{
    public sealed class PointTargetingShape : TargetingShape
    {
        public readonly int MaxRange;
        public int Range { get; set; }
        public Coord Offset { get; private set; }


        public PointTargetingShape(Coord origin, int maxRange = 0) : base(origin)
        {
            MaxRange = Range = maxRange;
        }

        public override IEnumerable<Coord> GetPoints()
        {
            yield return Origin + Offset;
        }

        public override bool TryOffset(Coord offs)
        {
            var newOffset = Offset + offs;
            if (Math.Abs((Origin + newOffset).DistChebyshev(Origin)) > MaxRange)
            {
                return false;
            }
            Offset = newOffset;
            OnChanged();
            return true;
        }

        public override bool CanRotateWithDirectionKeys() => false;
        public override bool TryRotateCCw() => false;
        public override bool TryRotateCw() => false;
        public override bool CanExpandWithDirectionKeys() => false;
        public override bool TryContract() => false;
        public override bool TryExpand() => false;

        public override bool TryAutoTarget(Func<Coord, bool> validTarget, Func<Coord, bool> obstacle)
        {
            foreach (var p in Shapes.Box(Origin, MaxRange))
            {
                if (obstacle(p))
                {
                    continue;
                }
                if (validTarget(p))
                {
                    Offset = p - Origin;
                    OnChanged();
                    return true;
                }
            }
            return false;
        }
    }
}
