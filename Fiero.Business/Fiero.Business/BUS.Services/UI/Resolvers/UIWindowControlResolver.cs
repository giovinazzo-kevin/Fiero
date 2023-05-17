using Fiero.Core;

namespace Fiero.Business
{
    [SingletonDependency(typeof(IUIControlResolver<UIWindowControl>))]
    public class UIWindowControlResolver : UIControlResolver<UIWindowControl>
    {
        public UIWindowControlResolver(GameUI ui, GameResources resources)
            : base(ui, resources)
        {
        }

        public override UIWindowControl Resolve(LayoutGrid dom)
        {
            var x = new UIWindowControl(UI.Input);
            return x;
        }
    }
}
