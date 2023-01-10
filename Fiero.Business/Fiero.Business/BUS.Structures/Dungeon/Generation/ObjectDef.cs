using Fiero.Core;
using System;

namespace Fiero.Business
{
    public class ObjectDef
    {
        public readonly string Name;
        public readonly Coord Position;
        public readonly Func<FloorId, PhysicalEntity> Build;
        public ObjectDef(string name, Coord pos, Func<FloorId, PhysicalEntity> build)
            => (Build, Name, Position) = (build, name, pos);
    }
}
