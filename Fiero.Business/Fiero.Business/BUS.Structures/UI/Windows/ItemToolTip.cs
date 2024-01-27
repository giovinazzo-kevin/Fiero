using LightInject;

namespace Fiero.Business
{
    [TransientDependency]
    public class ItemToolTip : ToolTip
    {
        public readonly LayoutRef<Layout> BackgroundPane = new();
        public readonly LayoutRef<Label> ItemTitle = new();
        public readonly LayoutRef<Paragraph> ItemDesc = new();
        public readonly LayoutRef<Picture> Picture = new();

        public readonly UIControlProperty<Item> Item = new(nameof(Item));

        private readonly GameResources resources;

        private void Rescale()
        {
            var sizeX = Math.Max(
                ItemDesc.Control.MinimumContentSize.X + Picture.Control.Size.V.X,
                ItemTitle.Control.MinimumContentSize.X
            );
            Layout.Size.V = new(sizeX, Math.Max(48, Layout.Size.V.Y));
        }

        public ItemToolTip(GameUI ui) : base(ui)
        {
            resources = ui.ServiceProvider.GetInstance<GameResources>();
            Item.ValueChanged += (_, old) =>
            {
                if (Item.V == null)
                    return;
                ItemTitle.Control.Text.V = Item.V.Info.Name?.ToUpper() ?? string.Empty;
                ItemDesc.Control.Text.V = Item.V.Info.Description ?? string.Empty;
                Picture.Control.Sprite.V = resources.Sprites
                    .Get(TextureName.Items, Item.V.ItemProperties.ItemSprite ?? Item.V.Render.Sprite, Item.V.Render.Color);
                Rescale();
            };
        }

        protected override LayoutThemeBuilder DefineStyles(LayoutThemeBuilder builder) => base.DefineStyles(builder)
            .Rule<UIControl>(b => b
                .Match(x => x.HasClass("padded"))
                .Apply(x => x.Padding.V = new(4, 4)))
            .Rule<UIControl>(b => b
                .Match(x => x.HasClass("centered"))
                .Apply(x => x.HorizontalAlignment.V = HorizontalAlignment.Center))
            .Rule<UIControl>(b => b
                .Match(x => x.HasClass("border"))
                .Apply(x =>
                {
                    x.OutlineColor.V = resources.Colors.Get(ColorName.UIBorder);
                    x.OutlineThickness.V = 1;
                }))
            ;

        protected override LayoutGrid RenderContent(LayoutGrid grid) => base.RenderContent(grid)
            .Row(@class: "tooltip border")
                .Cell(BackgroundPane)
                .Col()
                    .Row(h: 16, px: true, @class: "centered")
                        .Cell(ItemTitle)
                    .End()
                    .Row(h: 32, px: true)
                        .Col(w: 32, px: true)
                            .Cell(Picture)
                        .End()
                        .Col(@class: "padded")
                            .Cell(ItemDesc)
                        .End()
                    .End()
                .End()
            .End();

    }
}
