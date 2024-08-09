using Fiero.Core;
using SFML.Graphics;

namespace Fiero.Business
{
    [UIResolver<Header>]
    public class HeaderResolver : UIControlResolver<Header>
    {
        public HeaderResolver(GameUI ui, GameResources resources)
            : base(ui, resources)
        {
        }
        public override Header Resolve()
        {
            var x = new Header(UI.Input,
                GetSprite(TextureName.UI, "header-l", ColorName.White),
                GetSprite(TextureName.UI, "header-m", ColorName.White),
                GetSprite(TextureName.UI, "header-r", ColorName.White));
            x.Font.V = GetFont(FontName.Light);
            x.Foreground.V = Foreground;
            x.Background.V = Color.White;
            x.ContentAwareScale.V = false;
            x.FontSize.V = x.Font.V.Size;
            return x;
        }
    }
}
