using Fiero.Core;

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
            var x = new ProgressBar(UI.Input,
                GetUISprite("bar_empty-l", ColorName.White),
                GetUISprite("bar_empty-m", ColorName.White),
                GetUISprite("bar_empty-r", ColorName.White),
                GetUISprite("bar_fill", ColorName.White));
            x.Foreground.V = Foreground;
            x.Background.V = Background;
            return x;
        }
    }
}
