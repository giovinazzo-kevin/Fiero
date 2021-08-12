using Fiero.Core;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace Fiero.Business
{
    public readonly struct TargetingShape
    {
        private readonly Coord _origin;

        public readonly Coord[] Points;
        public readonly int MaximumShiftRange;
        public readonly bool CanBeRotated;

        public TargetingShape(int shiftRange, bool canRotate, params Coord[] points)
        {
            _origin = new();
            Points = points;
            MaximumShiftRange = shiftRange;
            CanBeRotated = canRotate;
        }

        private TargetingShape(Coord origin, int shiftRange, bool canRotate, params Coord[] points)
        {
            _origin = origin;
            Points = points;
            MaximumShiftRange = shiftRange;
            CanBeRotated = canRotate;
        }

        public bool TryRotate(int deg, out TargetingShape rotated)
        {
            rotated = default;
            if (!CanBeRotated)
                return false;
            var rad = deg * (float)Math.PI / 180;
            var points = Points.Select(p => p.ToVec().Rotate(rad).ToCoord()).ToArray();
            rotated = new(_origin, MaximumShiftRange, CanBeRotated, points);
            return true;
        }

        public bool TryOffset(Coord offs, out TargetingShape offset)
        {
            offset = default;
            var newOrigin = _origin + offs;
            if (newOrigin.X < -MaximumShiftRange || newOrigin.Y < -MaximumShiftRange
                || newOrigin.X > MaximumShiftRange || newOrigin.Y > MaximumShiftRange)
                return false;
            var points = Points.Select(p => (p + offs)).ToArray();
            offset = new(newOrigin, MaximumShiftRange, CanBeRotated, points);
            return true;
        }

        public static TargetingShape Offset(TargetingShape shape, Coord offs)
        {
            var points = shape.Points.Select(p => (p + offs)).ToArray();
            return new(shape._origin, shape.MaximumShiftRange, shape.CanBeRotated, points);
        }
    }
}
