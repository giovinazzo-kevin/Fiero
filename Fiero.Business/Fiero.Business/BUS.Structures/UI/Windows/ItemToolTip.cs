namespace Fiero.Business
{
    public class ItemToolTip : ToolTip
    {
        public readonly LayoutRef<Layout> BackgroundPane;

        public ItemToolTip(GameUI ui) : base(ui)
        {
        }

        protected override LayoutGrid RenderContent(LayoutGrid grid) => base.RenderContent(grid)
            .Row()
                .Cell(BackgroundPane)
                .Col()

                .End()
            .End();

    }
}
