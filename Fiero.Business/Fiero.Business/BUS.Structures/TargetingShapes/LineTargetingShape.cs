namespace Fiero.Business
{
    public class LineTargetingShape : RayTargetingShape
    {
        public readonly int MinLength, MaxLength;

        public LineTargetingShape(Coord origin, int minLength, int maxLength)
            : base(origin, maxLength)
        {
            MaxLength = maxLength;
            MinLength = minLength;
        }

        public override IEnumerable<Coord> GetPoints()
        {
            return Shapes.Line(Origin, Origin + RayTip).Skip(1);
        }

        public override bool TryOffset(Coord offs)
        {
            return false;
        }

        public override bool CanExpandWithDirectionKeys() => true;
        public override bool TryContract()
        {
            if (Length > MinLength)
                Length--;
            OnChanged();
            return true;
        }
        public override bool TryExpand()
        {
            if (Length < MaxLength)
                Length++;
            OnChanged();
            return true;
        }

        public override bool TryAutoTarget(Func<Coord, bool> validTarget, Func<Coord, bool> obstacle)
        {
            var l = Length;
            for (int i = 0; i < 8; i++)
            {
                for (Length = MinLength; Length < MaxLength; Length++)
                {
                    OnChanged();
                    foreach (var p in GetPoints())
                    {
                        if (obstacle(p))
                            goto next_dir;
                        if (validTarget(p))
                            return true;
                    }
                }
            next_dir:
                TryRotateCw();
            }
            Length = l;
            OnChanged();
            return false;
        }
    }
}
