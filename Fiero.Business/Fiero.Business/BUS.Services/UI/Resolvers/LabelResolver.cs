using Fiero.Core;
using SFML.Graphics;

namespace Fiero.Business
{
    [SingletonDependency(typeof(IUIControlResolver<Label>))]
    public class LabelResolver : UIControlResolver<Label>
    {
        public LabelResolver(GameUI ui, GameResources resources)
            : base(ui, resources)
        {
        }

        public override Label Resolve(LayoutGrid dom)
        {
            var x = new Label(UI.Input, GetText);
            x.Foreground.V = Foreground;
            x.Background.V = Color.Transparent;
            x.ContentAwareScale.V = false;
            x.FontSize.V = 8;
            return x;
        }
    }
}
