using Fiero.Core;

namespace Fiero.Business
{
    [SingletonDependency(typeof(IUIControlResolver<Viewport>))]
    public class ViewportResolver : UIControlResolver<Viewport>
    {
        protected readonly DungeonSystem FloorSystem;
        protected readonly FactionSystem FactionSystem;

        public ViewportResolver(GameUI ui, GameResources resources, FactionSystem fac, DungeonSystem floorSystem)
            : base(ui, resources)
        {
            FloorSystem = floorSystem;
            FactionSystem = fac;
        }

        public override Viewport Resolve(LayoutGrid dom)
        {
            var view = new Viewport(UI.Input, FloorSystem, FactionSystem, Resources);
            view.Background.V = Background;
            view.Foreground.V = Foreground;
            return view;
        }
    }
}
