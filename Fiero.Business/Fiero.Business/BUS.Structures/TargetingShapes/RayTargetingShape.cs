namespace Fiero.Business
{

    public class RayTargetingShape : TargetingShape
    {
        public int Length { get; protected set; }

        public Coord RayTip { get; protected set; }
        public float Rotation { get; protected set; }

        public RayTargetingShape(Coord origin, int length) : base(origin)
        {
            Length = length;
            RayTip = new(Length, 0);
        }

        public override IEnumerable<Coord> GetPoints()
        {
            return Shapes.Line(Origin, Origin + RayTip).Skip(1);
        }

        public override bool TryOffset(Coord offs)
        {
            return false;
        }

        protected override void OnChanged()
        {
            RayTip = new Coord(
                (int)Math.Round(Math.Cos(Rotation) * Length),
                (int)Math.Round(Math.Sin(Rotation) * Length)
            );
            base.OnChanged();
        }


        private void Rotate(int deg)
        {
            Rotation += deg * (float)Math.PI / 180;
            OnChanged();
        }

        public override bool CanRotateWithDirectionKeys() => true;

        public override bool CanExpandWithDirectionKeys() => false;
        public override bool TryContract() => false;
        public override bool TryExpand() => false;

        public override bool TryRotateCCw()
        {
            Rotate(-45);
            OnChanged();
            return true;
        }

        public override bool TryRotateCw()
        {
            Rotate(45);
            OnChanged();
            return true;
        }

        public override bool TryAutoTarget(Func<Coord, bool> validTarget, Func<Coord, bool> obstacle)
        {
            for (int i = 0; i < 8; i++)
            {
                foreach (var p in GetPoints())
                {
                    if (obstacle(p))
                    {
                        break;
                    }
                    if (validTarget(p))
                    {
                        return true;
                    }
                }
                TryRotateCw();
            }
            return false;
        }
    }
}
