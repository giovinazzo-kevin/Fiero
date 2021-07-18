using Fiero.Core;
using System.Collections.Generic;

namespace Fiero.Business
{
    public class AIComponent : EcsComponent
    {
        public LinkedList<MapCell> Path { get; set; }
        public Actor Target { get; set; }
    }
}
