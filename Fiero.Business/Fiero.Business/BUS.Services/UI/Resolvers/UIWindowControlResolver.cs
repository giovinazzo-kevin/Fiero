using Fiero.Core;

namespace Fiero.Business
{
    [UIResolver<UIWindowAsControl>]
    public class UIWindowControlResolver : UIControlResolver<UIWindowAsControl>
    {
        public UIWindowControlResolver(GameUI ui, GameResources resources)
            : base(ui, resources)
        {
        }

        public override UIWindowAsControl Resolve()
        {
            var x = new UIWindowAsControl(UI.Input);
            return x;
        }
    }
}
