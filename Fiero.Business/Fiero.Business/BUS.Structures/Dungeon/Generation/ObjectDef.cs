namespace Fiero.Business
{
    public class ObjectDef
    {
        public readonly string Name;
        public readonly Type Type;
        public readonly bool IsFeature;
        public readonly Coord Position;
        public readonly object Data;
        public readonly Func<FloorId, PhysicalEntity> Build;
        public ObjectDef(string name, Type type, bool isFeature, Coord pos, object data, Func<FloorId, PhysicalEntity> build)
            => (Build, Data, Type, IsFeature, Name, Position) = (build, data, type, isFeature, name, pos);
    }
}
