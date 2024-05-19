using SFML.Graphics;

namespace Fiero.Business
{
    [UIResolver<Picture>]
    public class PictureResolver : UIControlResolver<Picture>
    {
        public PictureResolver(GameUI ui, GameResources resources)
            : base(ui, resources)
        {
        }

        public override Picture Resolve()
        {
            var x = new Picture(UI.Input);
            x.Foreground.V = Foreground;
            x.Background.V = Color.Transparent;
            return x;
        }
    }
}
