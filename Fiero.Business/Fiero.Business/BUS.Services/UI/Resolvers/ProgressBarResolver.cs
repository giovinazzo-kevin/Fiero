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
                GetUISprite("bar_empty-l", ColorName.White), GetUISprite("bar_empty-m", ColorName.White), GetUISprite("bar_empty-r", ColorName.White),
                GetUISprite("bar_half-l", ColorName.White), GetUISprite("bar_half-m", ColorName.White), GetUISprite("bar_half-r", ColorName.White),
                GetUISprite("bar_full-l", ColorName.White), GetUISprite("bar_full-m", ColorName.White), GetUISprite("bar_full-r", ColorName.White));
            x.Foreground.V = Foreground;
            x.Background.V = Background;
            return x;
        }
    }
}
