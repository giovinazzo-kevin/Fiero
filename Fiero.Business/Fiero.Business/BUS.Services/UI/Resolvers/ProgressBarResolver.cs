using Fiero.Core;
using SFML.Graphics;

namespace Fiero.Business
{
    [SingletonDependency(typeof(IUIControlResolver<ProgressBar>))]
    public class ProgressBarResolver : UIControlResolver<ProgressBar>
    {
        public ProgressBarResolver(GameUI ui, GameResources resources)
            : base(ui, resources)
        {
        }
        public override ProgressBar Resolve(LayoutGrid dom)
        {
            var x = new ProgressBar(UI.Input, TileSize,
                GetUISprite("bar_empty-l"), GetUISprite("bar_empty-m"), GetUISprite("bar_empty-r"),
                GetUISprite("bar_half-l"), GetUISprite("bar_half-m"), GetUISprite("bar_half-r"),
                GetUISprite("bar_full-l"), GetUISprite("bar_full-m"), GetUISprite("bar_full-r"));
            x.Foreground.V = Foreground;
            x.Background.V = Color.Transparent;
            return x;
        }
    }
}
