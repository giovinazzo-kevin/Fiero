using Fiero.Core;

namespace Fiero.Business
{
    [SingletonDependency(typeof(IUIControlResolver<ComboBox>))]
    public class ComboboxResolver : UIControlResolver<ComboBox>
    {
        public ComboboxResolver(GameUI ui, GameResources resources)
            : base(ui, resources)
        {
        }

        public override ComboBox Resolve(LayoutGrid dom)
        {
            var x = new ComboBox(UI.Input, () => new(UI.Input));
            x.Font.V = GetFont();
            x.Foreground.V = Foreground;
            x.Background.V = Background;
            x.Accent.V = Accent;
            return x;
        }
    }
}
