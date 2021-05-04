using SFML.Graphics;
using System;

namespace Fiero.Core
{

    public class GameUI<TFonts, TTextures, TSounds>
        where TFonts : struct, Enum
        where TTextures : struct, Enum
        where TSounds : struct, Enum
    {
        protected readonly GameSprites<TTextures> Sprites;
        protected readonly GameFonts<TFonts> Fonts;
        protected readonly GameSounds<TSounds> Sounds;
        protected readonly GameInput Input;

        public GameUI(GameInput input, GameFonts<TFonts> fonts, GameSprites<TTextures> sprites, GameSounds<TSounds> sounds)
        {
            Sprites = sprites;
            Fonts = fonts;
            Sounds = sounds;
            Input = input;
        }

        public LayoutBuilder<TFonts, TTextures, TSounds> CreateLayout() 
            => new(Input, Fonts, Sprites, Sounds);
    }
}
