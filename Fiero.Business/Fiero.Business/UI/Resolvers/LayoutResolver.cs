using Fiero.Core;

namespace Fiero.Business
{
    public class LayoutResolver : UIControlResolver<Layout>
    {
        public LayoutResolver(
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

        public override Layout Resolve(LayoutGrid dom)
        {
            var x = new Layout(dom, Input);
            x.Foreground.V = Foreground;
            x.Background.V = Background;
            return x;
        }
    }
}
