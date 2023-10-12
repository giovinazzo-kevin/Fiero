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
        public readonly ScriptingSystem Scripting;
        public readonly MetaSystem Meta;
        public readonly MusicSystem Music;
        public readonly InputSystem Input;

        public GameSystems(
            ActionSystem action,
            DialogueSystem dialogue,
            FactionSystem faction,
            DungeonSystem dungeon,
            RenderSystem render,
            ScriptingSystem scripting,
            MetaSystem meta,
            MusicSystem music,
            InputSystem input

        )
        {
            Action = action;
            Dialogue = dialogue;
            Faction = faction;
            Dungeon = dungeon;
            Render = render;
            Scripting = scripting;
            Meta = meta;
            Music = music;
            Input = input;
        }
    }
}
