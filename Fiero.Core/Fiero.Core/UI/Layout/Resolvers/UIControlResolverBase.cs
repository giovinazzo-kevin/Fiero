using System;
using System.Linq;

namespace Fiero.Core
{
    public abstract class UIControlResolverBase<T, TFonts, TTextures, TLocales, TSounds, TColors> : IUIControlResolver<T>
        where TTextures : struct, Enum
        where TLocales : struct, Enum
        where TSounds : struct, Enum
        where TColors : struct, Enum
        where TFonts : struct, Enum
        where T : UIControl
    {
        private static readonly string[] _frameLabels = new[] {
            "tl", "tm", "tr", "l", "m", "r", "bl", "bm", "br"
        };

        protected GameUI UI;
        protected GameInput Input;
        protected GameDataStore Store;
        protected GameFonts<TFonts> Fonts;
        protected GameSounds<TSounds> Sounds;
        protected GameSprites<TTextures> Sprites;
        protected GameColors<TColors> Colors;
        protected GameLocalizations<TLocales> Localizations;

        public UIControlResolverBase(
            GameUI ui,
            GameInput input,
            GameDataStore store,
            GameFonts<TFonts> fonts,
            GameSounds<TSounds> sounds,
            GameColors<TColors> colors,
            GameSprites<TTextures> sprites,
            GameLocalizations<TLocales> localizations
        ) {
            UI = ui;
            Input = input;
            Store = store;
            Fonts = fonts;
            Sounds = sounds;
            Colors = colors;
            Sprites = sprites;
            Localizations = localizations;
        }

        public abstract T Resolve(LayoutGrid dom, Coord position, Coord size);

        //protected virtual Frame CreateFrame(TTextures texture, string sprite, Coord size, int tileSize)
        //{
        //    var sprites = _frameLabels.Select(l => Sprites.TryGet(texture, $"{sprite}-{l}", out var s) ? s : null)
        //        .ToArray();
        //    return new Frame(Input, tileSize, sprites) {
        //        Size = size
        //    };
        //}
    }
}
