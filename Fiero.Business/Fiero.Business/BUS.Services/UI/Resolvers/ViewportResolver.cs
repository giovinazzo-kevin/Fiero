using Fiero.Core;

namespace Fiero.Business
{
    [SingletonDependency(typeof(IUIControlResolver<Viewport>))]
    public class ViewportResolver : UIControlResolver<Viewport>
    {
        protected readonly FloorSystem FloorSystem;

        public ViewportResolver(GameUI ui, GameResources resources, FloorSystem floorSystem)
            : base(ui, resources)
        {
            FloorSystem = floorSystem;
        }

        public override Viewport Resolve(LayoutGrid dom)
        {
            var view = new Viewport(UI.Input, FloorSystem, Resources.Sprites, Resources.Colors);
            view.Background.V = Background;
            view.Foreground.V = Foreground;
            return view;
        }
    }
}
