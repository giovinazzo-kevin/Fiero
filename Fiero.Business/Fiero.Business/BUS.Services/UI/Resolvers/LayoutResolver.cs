using Fiero.Core;

namespace Fiero.Business
{
    [SingletonDependency(typeof(IUIControlResolver<Layout>))]
    public class LayoutResolver : UIControlResolver<Layout>
    {
        public LayoutResolver(GameUI ui, GameResources resources)
            : base(ui, resources)
        {
        }

        public override Layout Resolve(LayoutGrid dom)
        {
            var x = new Layout(dom, UI.Input);
            x.Foreground.V = Foreground;
            x.Background.V = Background;
            return x;
        }
    }
}
