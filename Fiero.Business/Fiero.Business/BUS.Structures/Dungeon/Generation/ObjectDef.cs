using Fiero.Core;
using System;

namespace Fiero.Business
{
    public class ObjectDef
    {
        public readonly string Name;
        public readonly bool IsFeature;
        public readonly Coord Position;
        public readonly Func<FloorId, PhysicalEntity> Build;
        public ObjectDef(string name, bool isFeature, Coord pos, Func<FloorId, PhysicalEntity> build)
            => (Build, IsFeature, Name, Position) = (build, isFeature, name, pos);
    }
}
