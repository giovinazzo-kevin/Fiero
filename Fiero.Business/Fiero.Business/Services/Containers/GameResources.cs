using Fiero.Core;

namespace Fiero.Business
{
    [SingletonDependency]
    public class GameResources
    {
        public readonly GameColors<ColorName> Colors;
        public readonly GameLocalizations<LocaleName> Localizations;
        public readonly GameShaders<ShaderName> Shaders;
        public readonly GameSounds<SoundName> Sounds;
        public readonly GameSprites<TextureName> Sprites;
        public readonly GameTextures<TextureName> Textures;
        public readonly GameGlossaries Glossaries;
        public readonly GameDialogues Dialogues;
        public readonly GameEntityBuilders Entities;

        public GameResources(
            GameColors<ColorName> colors,
            GameLocalizations<LocaleName> localizations,
            GameShaders<ShaderName> shaders,
            GameSounds<SoundName> sounds,
            GameSprites<TextureName> sprites,
            GameTextures<TextureName> textures,
            GameGlossaries glossaries,
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
            Glossaries = glossaries;
            Dialogues = dialogues;
            Entities = entities;
        }
    }
}
