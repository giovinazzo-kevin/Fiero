using Fiero.Core;
using SFML.Graphics;

namespace Fiero.Business
{
    [SingletonDependency(typeof(IUIControlResolver<ProgressBar>))]
    public class ProgressBarResolver : UIControlResolver<ProgressBar>
    {
        public ProgressBarResolver(
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
        }
        public override ProgressBar Resolve(LayoutGrid dom)
        {
            var x = new ProgressBar(Input, TileSize,
                GetUISprite("bar_empty-l"), GetUISprite("bar_empty-m"), GetUISprite("bar_empty-r"),
                GetUISprite("bar_half-l"), GetUISprite("bar_half-m"), GetUISprite("bar_half-r"),
                GetUISprite("bar_full-l"), GetUISprite("bar_full-m"), GetUISprite("bar_full-r"));
            x.Foreground.V = Foreground;
            x.Background.V = Color.Transparent;
            return x;
        }
    }
}
