using Fiero.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    public class RayTargetingShape : TargetingShape
    {
        public readonly int Length;

        public Coord RayTip { get; private set; }
        public float Rotation { get; private set; }

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

        private void Rotate(int deg)
        {
            Rotation += deg * (float)Math.PI / 180;
            RayTip = new Coord(
                (int)Math.Round(Math.Cos(Rotation) * Length), 
                (int)Math.Round(Math.Sin(Rotation) * Length)
            );
        }

        public override bool CanRotateWithDirectionKeys() => true;

        public override bool TryRotateCCw()
        {
            Rotate(-45);
            return true;
        }

        public override bool TryRotateCw()
        {
            Rotate(45);
            return true;
        }

        public override bool TryAutoTarget(Func<Coord, bool> validTarget, Func<Coord, bool> obstacle)
        {
            for (int i = 0; i < 8; i++) {
                foreach (var p in GetPoints()) {
                    if (obstacle(p)) {
                        break;
                    }
                    if(validTarget(p)) {
                        return true;
                    }
                }
                TryRotateCw();
            }
            return false;
        }
    }
}
