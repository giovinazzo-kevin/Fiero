using Fiero.Core;

namespace Fiero.Business
{
    [SingletonDependency(typeof(IUIControlResolver<Combobox>))]
    public class ComboboxResolver : UIControlResolver<Combobox>
    {
        public ComboboxResolver(
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

        public override Combobox Resolve(LayoutGrid dom)
        {
            var x = new Combobox(Input, GetText, () => new(Input, GetText));
            x.Foreground.V = Foreground;
            x.Background.V = Background;
            x.Accent.V = Accent;
            return x;
        }
    }
}
