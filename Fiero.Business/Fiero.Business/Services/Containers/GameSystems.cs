using Fiero.Core;
using System.Collections.Generic;
using Unconcern.Common;

namespace Fiero.Business
{
    [SingletonDependency]
    public class GameSystems
    {
        public readonly ActionSystem Action;
        public readonly DialogueSystem Dialogue;
        public readonly FactionSystem Faction;
        public readonly FloorSystem Floor;
        public readonly RenderSystem Render;

        public GameSystems(
            ActionSystem action,
            DialogueSystem dialogue,
            FactionSystem faction,
            FloorSystem floor,
            RenderSystem render
        )
        {
            Action = action;
            Dialogue = dialogue;
            Faction = faction;
            Floor = floor;
            Render = render;
        }
    }
}
