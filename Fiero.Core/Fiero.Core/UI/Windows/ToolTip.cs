namespace Fiero.Core
{
    [TransientDependency]
    public abstract class ToolTip : UIWindow
    {
        public TimeSpan DisplayTimeout { get; set; } = TimeSpan.FromSeconds(0.5);

        public ToolTip(GameUI ui) : base(ui)
        {
        }

        public override void Update(TimeSpan t, TimeSpan dt)
        {
            if (IsOpen)
            {
                Layout.Position.V = UI.Input.GetMousePosition();
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
