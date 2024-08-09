namespace Fiero.Business
{
    [UIResolver<ProgressBar>]
    public class ProgressBarResolver : UIControlResolver<ProgressBar>
    {
        public ProgressBarResolver(GameUI ui, GameResources resources)
            : base(ui, resources)
        {
        }
        public override ProgressBar Resolve()
        {
            var x = new ProgressBar(UI.Input,
                GetSprite(TextureName.UI, "bar_empty-l", ColorName.White),
                GetSprite(TextureName.UI, "bar_empty-m", ColorName.White),
                GetSprite(TextureName.UI, "bar_empty-r", ColorName.White),
                GetSprite(TextureName.UI, "bar_fill", ColorName.White));
            x.Foreground.V = Foreground;
            x.Background.V = Background;
            return x;
        }
    }
}
