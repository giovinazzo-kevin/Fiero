using Ergo.Lang.Extensions;
using LightInject;

namespace Fiero.Business
{
    [TransientDependency]
    public class CellToolTip : ToolTip
    {
        public readonly LayoutRef<Layout> BackgroundPane = new();
        public readonly LayoutRef<Paragraph> CellDesc = new();
        public readonly LayoutRef<Picture> Picture = new();

        public UIControlProperty<MapCell> Cell {get; private set;} = new(nameof(MapCell));

        private readonly GameResources resources;

        private void Rescale()
        {
            var sizeX = CellDesc.Control.MinimumContentSize.X + Picture.Control.Size.V.X;
            var sizeY = CellDesc.Control.MinimumContentSize.Y + Picture.Control.Size.V.Y;
            Layout.Size.V = new(sizeX, sizeY);
        }

        public CellToolTip(GameUI ui) : base(ui)
        {
            resources = ui.ServiceProvider.GetInstance<GameResources>();
            Cell.ValueChanged += (_, old) =>
            {
                if (Cell.V == null)
                {
                    CellDesc.Control.Text.V = "???";
                    Picture.Control.Sprite.V = default;
                    Rescale();
                    return;
                }
                var thingsHere = $"Things here:\n- {Cell.V.Tile.TileProperties.Name}";
                if (Cell.V.Actors.Any())
                {
                    thingsHere += $"\n- {Cell.V.Actors.Select(a => a.Info.Name).Join(", ")}";
                }
                if (Cell.V.Items.Any())
                {
                    thingsHere += $"\n- {Cell.V.Items.Select(a => a.Info.Name).Join(", ")}";
                }
                if (Cell.V.Features.Any())
                {
                    thingsHere += $"\n- {Cell.V.Features.Select(a => a.Info.Name).Join(", ")}";
                }
                CellDesc.Control.Text.V = thingsHere;
                Picture.Control.Sprite.V = new(TextureName.Tiles, Cell.V.Tile.Render.Sprite, Cell.V.Tile.Render.Color);
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
                    .Row(h: 64, px: true)
                        .Col(w: 32, px: true)
                            .Row(h: 32, px: true)
                                .Cell(Picture)
                            .End()
                            .Row(h: 32, px: true)
                                .Cell(Picture)
                            .End()
                        .End()
                        .Col(@class: "padded")
                            .Cell(CellDesc)
                        .End()
                    .End()
                .End()
            .End();

    }
}
