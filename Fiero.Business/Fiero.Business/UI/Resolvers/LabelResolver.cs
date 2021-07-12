using Fiero.Core;
using SFML.Graphics;
using System;

namespace Fiero.Business
{
    [SingletonDependency(typeof(IUIControlResolver<Label>))]
    public class LabelResolver : UIControlResolver<Label>
    {
        public LabelResolver(
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

        public override Label Resolve(LayoutGrid dom)
        {
            var x = new Label(Input, GetText);
            x.Foreground.V = Foreground;
            x.Background.V = Color.Transparent;
            x.ContentAwareScale.V = false;
            x.FontSize.V = 24;
            return x;
        }
    }
}
