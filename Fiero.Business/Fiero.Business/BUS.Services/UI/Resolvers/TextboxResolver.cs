using Fiero.Core;

namespace Fiero.Business
{
    [SingletonDependency(typeof(IUIControlResolver<Textbox>))]
    public class TextboxResolver : UIControlResolver<Textbox>
    {
        public TextboxResolver(GameUI ui, GameResources resources)
            : base(ui, resources)
        {
        }

        public override Textbox Resolve(LayoutGrid dom)
        {
            var x = new Textbox(UI.Input, GetText);
            x.Foreground.V = Foreground;
            x.Background.V = Background;
            x.ContentAwareScale.V = false;
            x.FontSize.V = 8;
            return x;
        }
    }
}
