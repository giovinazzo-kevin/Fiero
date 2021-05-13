using Fiero.Core;
using SFML.Audio;
using SFML.Graphics;

namespace Fiero.Business
{
    public abstract class UIControlResolver<T> : UIControlResolverBase<T, FontName, TextureName, LocaleName, SoundName, ColorName>
        where T : UIControl
    {
        protected readonly Color Foreground;
        protected readonly Color Background;
        protected readonly Color Accent;
        protected readonly int TileSize;

        protected UIControlResolver(GameInput input, GameDataStore store, GameFonts<FontName> fonts, GameSounds<SoundName> sounds, GameSprites<TextureName> sprites, GameLocalizations<LocaleName> localizations) : base(input, store, fonts, sounds, sprites, localizations)
        {
            Foreground = store.Get(Data.UI.DefaultForeground);
            Background = store.Get(Data.UI.DefaultBackground);
            Accent = store.Get(Data.UI.DefaultAccent);
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
