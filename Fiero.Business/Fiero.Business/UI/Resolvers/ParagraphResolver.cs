using Fiero.Core;
using SFML.Graphics;

namespace Fiero.Business
{
    public class ParagraphResolver : UIControlResolver<Paragraph>
    {
        public ParagraphResolver(
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

        public override Paragraph Resolve(LayoutGrid dom)
        {
            var x = new Paragraph(Input, GetText);
            x.Foreground.V = Foreground;
            x.Background.V = Color.Transparent;
            x.ContentAwareScale.V = false;
            x.CenterContentH.V = false;
            x.FontSize.V = 16;
            return x;
        }
    }
}
