using Fiero.Core;

namespace Fiero.Business
{
    [SingletonDependency(typeof(IUIControlResolver<Checkbox>))]
    public class CheckboxResolver : UIControlResolver<Checkbox>
    {
        public CheckboxResolver(GameUI ui, GameResources resources)
            : base(ui, resources)
        {
        }

        public override Checkbox Resolve(LayoutGrid dom)
        {
            var x = new Checkbox(UI.Input);
            x.Foreground.V = Foreground;
            x.Background.V = Background;
            x.Accent.V = Accent;
            return x;
        }
    }
}
