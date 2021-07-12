using Fiero.Core;

namespace Fiero.Business
{
    [SingletonDependency(typeof(IUIControlResolver<Button>))]
    public class ButtonResolver : UIControlResolver<Button>
    {
        public ButtonResolver(
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

        public override Button Resolve(LayoutGrid dom)
        {
            var x = new Button(Input, GetText);
            x.Foreground.V = Foreground;
            x.Background.V = Background;
            x.ContentAwareScale.V = false;
            x.FontSize.V = 24;
            return x;
        }
    }
}
