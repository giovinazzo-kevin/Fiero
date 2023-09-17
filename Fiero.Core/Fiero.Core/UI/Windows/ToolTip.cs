namespace Fiero.Core
{
    [TransientDependency]
    public abstract class ToolTip : UIWindow
    {
        public TimeSpan DisplayTimeout { get; set; } = TimeSpan.FromSeconds(0.5);

        public ToolTip(GameUI ui) : base(ui)
        {
        }

        protected override void DefaultSize() { }

        public override void Update(TimeSpan t, TimeSpan dt)
        {
            if (IsOpen)
            {
                Layout.Position.V = UI.Input.GetMousePosition() - Layout.Size.V * Coord.PositiveY;
            }
            base.Update(t, dt);
        }

        protected override LayoutGrid RenderContent(LayoutGrid grid)
        {
            return grid
                .Cell<Layout>()
            ;
        }
    }
}
