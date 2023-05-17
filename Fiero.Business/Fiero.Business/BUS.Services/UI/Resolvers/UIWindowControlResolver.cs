using Fiero.Core;

namespace Fiero.Business
{
    [SingletonDependency(typeof(IUIControlResolver<UIWindowAsControl>))]
    public class UIWindowControlResolver : UIControlResolver<UIWindowAsControl>
    {
        public UIWindowControlResolver(GameUI ui, GameResources resources)
            : base(ui, resources)
        {
        }

        public override UIWindowAsControl Resolve(LayoutGrid dom)
        {
            var x = new UIWindowAsControl(UI.Input);
            return x;
        }
    }
}
