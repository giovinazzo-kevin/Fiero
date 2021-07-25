using Fiero.Core;
using SFML.Graphics;

namespace Fiero.Business
{
    [SingletonDependency(typeof(IUIControlResolver<Paragraph>))]
    public class ParagraphResolver : UIControlResolver<Paragraph>
    {
        public ParagraphResolver(GameUI ui, GameResources resources)
            : base(ui, resources)
        {
        }

        public override Paragraph Resolve(LayoutGrid dom)
        {
            var x = new Paragraph(UI.Input, GetText);
            x.Foreground.V = Foreground;
            x.Background.V = Color.Transparent;
            x.ContentAwareScale.V = false;
            x.CenterContentH.V = false;
            x.FontSize.V = 16;
            return x;
        }
    }
}
