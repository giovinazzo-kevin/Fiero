namespace Fiero.Business
{
    [UIResolver<Layout>]
    public class LayoutResolver : UIControlResolver<Layout>
    {
        public LayoutResolver(GameUI ui, GameResources resources)
            : base(ui, resources)
        {
        }

        public override Layout Resolve()
        {
            var x = new Layout(new(LayoutPoint.RelativeOne, new()), UI.Input);
            x.Foreground.V = Foreground;
            x.Background.V = Background;
            return x;
        }
    }
}
