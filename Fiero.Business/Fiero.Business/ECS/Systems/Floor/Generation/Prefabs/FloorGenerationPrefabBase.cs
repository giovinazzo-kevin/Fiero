using Fiero.Core;
using System;

namespace Fiero.Business
{
    public abstract class FloorGenerationPrefabBase : IFloorGenerationPrefab
    {
        public abstract void Draw(FloorGenerationContext ctx);
    }
}
