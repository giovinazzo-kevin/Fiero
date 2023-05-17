using Fiero.Core;

namespace Fiero.Business
{
    [SingletonDependency(typeof(IUIControlResolver<Viewport>))]
    public class ViewportResolver : UIControlResolver<Viewport>
    {
        protected readonly DungeonSystem FloorSystem;
        protected readonly FactionSystem FactionSystem;
        protected readonly GameLoop Loop;

        public ViewportResolver(GameUI ui, GameResources resources, FactionSystem fac, DungeonSystem floorSystem, GameLoop loop)
            : base(ui, resources)
        {
            FloorSystem = floorSystem;
            FactionSystem = fac;
            Loop = loop;
        }

        public override Viewport Resolve(LayoutGrid dom)
        {
            var view = new Viewport(UI.Input, FloorSystem, FactionSystem, Resources, Loop);
            view.Background.V = Background;
            view.Foreground.V = Foreground;
            return view;
        }
    }
}
