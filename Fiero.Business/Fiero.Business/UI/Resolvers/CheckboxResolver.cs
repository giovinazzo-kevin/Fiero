using Fiero.Core;

namespace Fiero.Business
{
    [SingletonDependency(typeof(IUIControlResolver<Checkbox>))]
    public class CheckboxResolver : UIControlResolver<Checkbox>
    {
        public CheckboxResolver(
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

        public override Checkbox Resolve(LayoutGrid dom)
        {
            var x = new Checkbox(Input);
            x.Foreground.V = Foreground;
            x.Background.V = Background;
            x.Accent.V = Accent;
            return x;
        }
    }
}
