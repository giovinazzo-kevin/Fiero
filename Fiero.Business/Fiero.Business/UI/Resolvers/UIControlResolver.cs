using Fiero.Core;
using SFML.Audio;
using SFML.Graphics;

namespace Fiero.Business
{
    public abstract class UIControlResolver<T> : UIControlResolverBase<T, FontName, TextureName, LocaleName, SoundName, ColorName>
        where T : UIControl
    {
        protected readonly Color ActiveForeground;
        protected readonly Color InactiveForeground;
        protected readonly Color ActiveBackground;
        protected readonly Color InactiveBackground;
        protected readonly int TileSize;

        protected UIControlResolver(GameInput input, GameDataStore store, GameFonts<FontName> fonts, GameSounds<SoundName> sounds, GameSprites<TextureName> sprites, GameLocalizations<LocaleName> localizations) : base(input, store, fonts, sounds, sprites, localizations)
        {
            ActiveForeground = store.Get(Data.UI.DefaultActiveForeground);
            InactiveForeground = store.Get(Data.UI.DefaultInactiveForeground);
            ActiveBackground = store.Get(Data.UI.DefaultActiveBackground);
            InactiveBackground = store.Get(Data.UI.DefaultInactiveBackground);
            TileSize = store.Get(Data.UI.TileSize);
        }

        protected virtual Font GetFont()
        {
            return Fonts.Get(FontName.UI);
        }

        protected virtual Text GetText(string str, int fontSize)
        {
            return new Text(str, GetFont(), (uint)fontSize);
        }

        protected virtual Sprite GetSprite(string str)
        {
            return Sprites.TryGet(TextureName.UI, str, out var sprite) ? sprite : null;
        }

        protected virtual Sound GetSound(SoundName sound)
        {
            return Sounds.Get(sound);
        }
    }
}
