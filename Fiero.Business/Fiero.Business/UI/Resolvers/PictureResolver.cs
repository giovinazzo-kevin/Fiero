using Fiero.Core;
using SFML.Graphics;

namespace Fiero.Business
{
    [SingletonDependency(typeof(IUIControlResolver<Picture<TextureName>>))]
    public class PictureResolver : UIControlResolver<Picture<TextureName>>
    {
        public PictureResolver(GameUI ui, GameResources resources)
            : base(ui, resources)
        {
        }

        public override Picture<TextureName> Resolve(LayoutGrid dom)
        {
            var x = new Picture<TextureName>(UI.Input, GetSprite);
            x.Foreground.V = Foreground;
            x.Background.V = Color.Transparent;
            return x;
        }
    }
}
