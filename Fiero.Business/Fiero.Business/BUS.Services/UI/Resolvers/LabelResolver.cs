using SFML.Graphics;

namespace Fiero.Business
{
    [UIResolver<Label>]
    public class LabelResolver : UIControlResolver<Label>
    {
        public LabelResolver(GameUI ui, GameResources resources)
            : base(ui, resources)
        {
        }

        public override Label Resolve()
        {
            var x = new Label(UI.Input);
            x.Font.V = GetFont();
            x.Foreground.V = Foreground;
            x.Background.V = Color.Transparent;
            x.ContentAwareScale.V = false;
            x.FontSize.V = x.Font.V.Size;
            return x;
        }
    }
}
