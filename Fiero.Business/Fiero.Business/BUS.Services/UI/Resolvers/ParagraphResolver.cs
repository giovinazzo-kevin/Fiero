using SFML.Graphics;

namespace Fiero.Business
{
    [UIResolver<Paragraph>]
    public class ParagraphResolver : UIControlResolver<Paragraph>
    {
        public ParagraphResolver(GameUI ui, GameResources resources)
            : base(ui, resources)
        {
        }

        public override Paragraph Resolve()
        {
            var x = new Paragraph(UI.Input);
            x.Font.V = GetFont();
            x.Foreground.V = Foreground;
            x.Background.V = Color.Transparent;
            x.ContentAwareScale.V = false;
            x.CenterContentH.V = false;
            x.FontSize.V = x.Font.V.Size;
            return x;
        }
    }
}
