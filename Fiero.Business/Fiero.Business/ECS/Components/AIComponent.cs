using Fiero.Core;
using System.Collections.Generic;

namespace Fiero.Business
{
    public class AIComponent : EcsComponent
    {
        public LinkedList<Tile> Path { get; set; }
        public Actor Target { get; set; }
    }
}
