using Fiero.Core;
using System;
using System.Collections.Generic;

namespace Fiero.Business
{
    public abstract class TargetingShape
    {
        public readonly Coord Origin;
        public event Action<TargetingShape> Changed;
        public TargetingShape(Coord origin)
        {
            Origin = origin;
        }

        protected void OnChanged()
        {
            Changed?.Invoke(this);
        }
        public abstract bool TryAutoTarget(Func<Coord, bool> validTarget, Func<Coord, bool> obstacle);
        public abstract bool TryRotateCw();
        public abstract bool TryRotateCCw();
        public abstract bool CanRotateWithDirectionKeys();
        public abstract bool TryOffset(Coord offs);
        public abstract IEnumerable<Coord> GetPoints();
    }
}
