using Fiero.Core;
using SFML.Graphics;

namespace Fiero.Business
{
    [SingletonDependency(typeof(IUIControlResolver<Header>))]
    public class HeaderResolver : UIControlResolver<Header>
    {
        public HeaderResolver(GameUI ui, GameResources resources)
            : base(ui, resources)
        {
        }
        public override Header Resolve(LayoutGrid dom)
        {
            var x = new Header(UI.Input,
                GetUISprite("header-l", ColorName.White),
                GetUISprite("header-m", ColorName.White),
                GetUISprite("header-r", ColorName.White));
            x.Font.V = GetFont();
            x.Foreground.V = Foreground;
            x.Background.V = Color.White;
            x.ContentAwareScale.V = false;
            x.FontSize.V = x.Font.V.Size;
            return x;
        }
    }
}
