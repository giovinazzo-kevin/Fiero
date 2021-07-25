using Fiero.Core;

namespace Fiero.Business
{
    public class ObjectDef
    {
        public readonly DungeonObjectName Name;
        public readonly Coord Position;
        public ObjectDef(DungeonObjectName name, Coord pos) => (Name, Position) = (name, pos);
    }
}
