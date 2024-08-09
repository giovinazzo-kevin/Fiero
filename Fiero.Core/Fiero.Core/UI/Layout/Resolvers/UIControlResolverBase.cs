using Fiero.Core;
using SFML.Audio;
using SFML.Graphics;

namespace Fiero.Core
{
    public abstract class UIControlResolver<T> : IUIControlResolver<T>
        where T : UIControl
    {
        protected readonly GameUI UI;
        protected readonly GameResources Resources;

        protected readonly Color Foreground;
        protected readonly Color Background;
        protected readonly Color Accent;

        public Type Type => typeof(T);

        protected UIControlResolver(GameUI ui, GameResources resources)
        {
            UI = ui;
            Resources = resources;
            Foreground = UI.Store.Get(CoreData.View.DefaultForeground);
            Background = UI.Store.Get(CoreData.View.DefaultBackground);
            Accent = UI.Store.Get(CoreData.View.DefaultAccent);
        }

        public abstract T Resolve();

        protected virtual BitmapText GetText(string font, string str)
        {
            return new BitmapText(GetFont(font), str);
        }

        protected virtual Sprite GetSprite(string texture, string str, string color, int? seed = null)
        {
            return Resources.Sprites.TryGet(texture, str, color, out var sprite, seed) ? sprite : null;
        }

        protected virtual Sound GetSound(string sound)
        {
            return Resources.Sounds.Get(sound);
        }

        protected virtual Color GetColor(string color)
        {
            return Resources.Colors.Get(color);
        }
        protected virtual BitmapFont GetFont(string fontName)
        {
            return Resources.Fonts.Get(fontName);
        }
    }
}