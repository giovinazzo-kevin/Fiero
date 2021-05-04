using Fiero.Core;
using System.Drawing;

namespace Fiero.Business
{
    public readonly struct DungeonObject
    {
        public readonly DungeonObjectName Type;
        public readonly Coord Position;

        public DungeonObject(DungeonObjectName type, Coord pos)
        {
            Type = type;
            Position = pos;
        }
    }
}
