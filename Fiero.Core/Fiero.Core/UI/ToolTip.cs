namespace Fiero.Core
{
    [TransientDependency]
    public abstract class ToolTip : UIWindow
    {
        public ToolTip(GameUI ui) : base(ui)
        {
        }

        public override LayoutGrid CreateLayout(LayoutGrid grid, string title) => throw new System.NotImplementedException();

        protected override void DefaultSize()
        {

        }

        protected override LayoutGrid RenderContent(LayoutGrid grid)
        {
            return grid
                .Cell<Layout>()
            ;
        }
    }
}
