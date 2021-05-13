using Fiero.Core;

namespace Fiero.Business
{
    public class ParagraphResolver : UIControlResolver<Paragraph>
    {
        public ParagraphResolver(GameInput input, GameDataStore store, GameFonts<FontName> fonts, GameSounds<SoundName> sounds, GameSprites<TextureName> sprites, GameLocalizations<LocaleName> localizations)
            : base(input, store, fonts, sounds, sprites, localizations)
        {
        }

        public override Paragraph Resolve(Coord position, Coord size)
        {
            var x = new Paragraph(Input, GetText);
            x.Foreground.V = ActiveForeground;
            x.Background.V = ActiveBackground;
            x.Position.V = position;
            x.Size.V = size;
            x.ContentAwareScale.V = false;
            x.CenterContent.V = false;
            x.FontSize.V = 24;
            return x;
        }
    }
}
