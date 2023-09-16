namespace Fiero.Core
{
    [TransientDependency]
    public abstract class ToolTip : UIWindow
    {
        public TimeSpan DisplayTimeout { get; set; } = TimeSpan.FromSeconds(0.5);
        private TimeSpan _timeoutAcc;

        public ToolTip(GameUI ui) : base(ui)
        {
        }

        protected override void DefaultSize()
        {

        }

        public override void Update(TimeSpan t, TimeSpan dt)
        {
            base.Update(t, dt);
            if (IsOpen)
            {
                Layout.Position.V = UI.Input.GetMousePosition();
            }
        }

        protected override LayoutGrid RenderContent(LayoutGrid grid)
        {
            return grid
                .Cell<Layout>()
            ;
        }
    }
}
