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

        protected UIControlResolver(
            GameUI ui, 
            GameInput input, 
            GameDataStore store, 
            GameFonts<FontName> fonts, 
            GameSounds<SoundName> sounds, 
            GameColors<ColorName> colors,
            GameSprites<TextureName> sprites, 
            GameLocalizations<LocaleName> localizations) 
            : base(ui, input, store, fonts, sounds, colors, sprites, localizations)
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

        protected virtual Sprite GetUISprite(string str) => GetSprite(TextureName.UI, str);
        protected virtual Sprite GetAtlasSprite(string str) => GetSprite(TextureName.Atlas, str);
        protected virtual Sprite GetSprite(TextureName texture, string str)
        {
            return Sprites.TryGet(texture, str, out var sprite) ? sprite : null;
        }

        protected virtual Sound GetSound(SoundName sound)
        {
            return Sounds.Get(sound);
        }

        protected virtual Color GetColor(ColorName color)
        {
            return Colors.Get(color);
        }
    }
}
