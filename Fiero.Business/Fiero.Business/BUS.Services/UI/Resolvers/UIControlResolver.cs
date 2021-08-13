using Fiero.Core;
using SFML.Audio;
using SFML.Graphics;

namespace Fiero.Business
{
    public abstract class UIControlResolver<T> : UIControlResolverBase<T, FontName, TextureName, LocaleName, SoundName, ColorName>
        where T : UIControl
    {
        protected readonly GameResources Resources;

        protected readonly Color Foreground;
        protected readonly Color Background;
        protected readonly Color Accent;
        protected readonly int TileSize;

        protected UIControlResolver(
            GameUI ui, 
            GameResources resources) 
            : base(ui)
        {
            Resources = resources;
            Foreground = UI.Store.Get(Data.UI.DefaultForeground);
            Background = UI.Store.Get(Data.UI.DefaultBackground);
            Accent = UI.Store.Get(Data.UI.DefaultAccent);
            TileSize = UI.Store.Get(Data.UI.TileSize);
        }

        protected virtual BitmapFont GetFont()
        {
            return Resources.Fonts.Get(FontName.Bold);
        }

        protected virtual BitmapText GetText(string str)
        {
            return new BitmapText(GetFont(), str);
        }

        protected virtual Sprite GetUISprite(string str, ColorName color) => GetSprite(TextureName.UI, str, color);
        protected virtual Sprite GetSprite(TextureName texture, string str, ColorName color, int? seed = null)
        {
            return Resources.Sprites.TryGet(texture, str, color, out var sprite, seed) ? sprite : null;
        }

        protected virtual Sound GetSound(SoundName sound)
        {
            return Resources.Sounds.Get(sound);
        }

        protected virtual Color GetColor(ColorName color)
        {
            return Resources.Colors.Get(color);
        }
    }
}
