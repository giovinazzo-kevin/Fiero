using Fiero.Core;

namespace Fiero.Business
{
    [SingletonDependency]
    public class GameSystems
    {
        public readonly ActionSystem Action;
        public readonly DialogueSystem Dialogue;
        public readonly FactionSystem Faction;
        public readonly DungeonSystem Dungeon;
        public readonly RenderSystem Render;
        public readonly InterfaceSystem Interface;
        public readonly ErgoScriptingSystem Scripting;
        public readonly MetaSystem Meta;

        public GameSystems(
            ActionSystem action,
            DialogueSystem dialogue,
            FactionSystem faction,
            DungeonSystem dungeon,
            RenderSystem render,
            InterfaceSystem @interface,
            ErgoScriptingSystem scripting,
            MetaSystem meta

        )
        {
            Action = action;
            Dialogue = dialogue;
            Faction = faction;
            Dungeon = dungeon;
            Render = render;
            Interface = @interface;
            Scripting = scripting;
            Meta = meta;
        }
    }
}
