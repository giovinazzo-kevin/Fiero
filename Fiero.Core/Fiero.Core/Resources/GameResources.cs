using Fiero.Core;

namespace Fiero.Core
{
    [SingletonDependency]
    public class GameResources
    {
        public readonly GameColors Colors;
        public readonly GameLocalizations Localizations;
        public readonly GameShaders Shaders;
        public readonly GameSounds Sounds;
        public readonly GameSprites Sprites;
        public readonly GameTextures Textures;
        public readonly GameFonts Fonts;
        public readonly GameScripts Scripts;

        public GameResources(
            GameColors colors,
            GameLocalizations localizations,
            GameShaders shaders,
            GameSounds sounds,
            GameSprites sprites,
            GameTextures textures,
            GameFonts fonts,
            GameScripts scripts
        )
        {
            Colors = colors;
            Localizations = localizations;
            Shaders = shaders;
            Sounds = sounds;
            Sprites = sprites;
            Textures = textures;
            Fonts = fonts;
            Scripts = scripts;
        }
    }
}
