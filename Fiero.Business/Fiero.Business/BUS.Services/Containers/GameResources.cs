namespace Fiero.Business
{
    [SingletonDependency]
    public class GameResources
    {
        public readonly GameColors<ColorName> Colors;
        public readonly GameLocalizations<LocaleName> Localizations;
        public readonly GameShaders<ShaderName> Shaders;
        public readonly GameSounds<SoundName> Sounds;
        public readonly GameSprites<TextureName, ColorName> Sprites;
        public readonly GameTextures<TextureName> Textures;
        public readonly GameFonts<FontName> Fonts;
        public readonly GameScripts<ScriptName> Scripts;
        public readonly GameDialogues Dialogues;
        public readonly GameEntityBuilders Entities;

        public GameResources(
            GameColors<ColorName> colors,
            GameLocalizations<LocaleName> localizations,
            GameShaders<ShaderName> shaders,
            GameSounds<SoundName> sounds,
            GameSprites<TextureName, ColorName> sprites,
            GameTextures<TextureName> textures,
            GameFonts<FontName> fonts,
            GameScripts<ScriptName> scripts,
            GameDialogues dialogues,
            GameEntityBuilders entities
        )
        {
            Colors = colors;
            Localizations = localizations;
            Shaders = shaders;
            Sounds = sounds;
            Sprites = sprites;
            Textures = textures;
            Fonts = fonts;
            Dialogues = dialogues;
            Entities = entities;
            Scripts = scripts;
        }
    }
}
