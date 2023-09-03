using Fiero.Core;

namespace Fiero.Business
{
    [TransientDependency]
    public class EquipmentDisplay : Widget
    {
        protected override LayoutStyleBuilder DefineStyles(LayoutStyleBuilder builder) => base.DefineStyles(builder)
            .AddRule<Picture>(r => r
                .Match(l => l.HasAnyClass("equipment-slot"))
                .Apply(l =>
                {
                    l.OutlineThickness.V = 2;
                    l.OutlineColor.V = UI.GetColor(ColorName.Cyan);
                }))
            .AddRule<Label>(r => r
                .Match(l => l.HasAnyClass("neck", "back", "ring"))
                .Apply(l =>
                {
                    l.HorizontalAlignment.V = HorizontalAlignment.Right;
                }))
            ;

        protected override LayoutGrid RenderContent(LayoutGrid grid)
        {
            return grid
                .Col(w: 16, px: true, @class: "spacer").Cell<Layout>().End()
                .Col(w: 36, px: true)
                    .Row(@class: "spacer").Cell<Layout>().End()
                    .Row(h: 16, px: true, @class: "equipment-slot head label")
                        .Cell<Label>(l => l.Text.V = "Lorem ipsum")
                    .End()
                    .Row(h: 36, px: true, @class: "equipment-slot head picture")
                        .Cell<Picture>()
                    .End()
                    .Row(@class: "spacer").Cell<Layout>().End()
                    .Row(h: 16, px: true, @class: "equipment-slot torso label")
                        .Cell<Label>(l => l.Text.V = "Lorem ipsum")
                    .End()
                    .Row(h: 36, px: true, @class: "equipment-slot torso picture")
                        .Cell<Picture>()
                    .End()
                    .Row(@class: "spacer").Cell<Layout>().End()
                    .Row(h: 16, px: true, @class: "equipment-slot hands label")
                        .Cell<Label>(l => l.Text.V = "Lorem ipsum")
                    .End()
                    .Row(h: 36, px: true, @class: "equipment-slot hands picture")
                        .Cell<Picture>()
                    .End()
                    .Row(@class: "spacer").Cell<Layout>().End()
                    .Row(h: 16, px: true, @class: "equipment-slot feet label")
                        .Cell<Label>(l => l.Text.V = "Lorem ipsum")
                    .End()
                    .Row(h: 36, px: true, @class: "equipment-slot feet picture")
                        .Cell<Picture>()
                    .End()
                    .Row(@class: "spacer").Cell<Layout>().End()
                .End()
                .Col(@class: "spacer")
                    .Cell<Picture>(x =>
                    {
                        x.VerticalAlignment.V = VerticalAlignment.Middle;
                        x.HorizontalAlignment.V = HorizontalAlignment.Center;
                        x.LockAspectRatio.V = true;
                    })
                .End()
                .Col(w: 36, px: true)
                    .Row(@class: "spacer").Cell<Layout>().End()
                    .Row(h: 16, px: true, @class: "equipment-slot neck label")
                        .Cell<Label>(l => l.Text.V = "Lorem ipsum")
                    .End()
                    .Row(h: 36, px: true, @class: "equipment-slot neck picture")
                        .Cell<Picture>()
                    .End()
                    .Row(@class: "spacer").Cell<Layout>().End()
                    .Row(h: 16, px: true, @class: "equipment-slot back label")
                        .Cell<Label>(l => l.Text.V = "Lorem ipsum")
                    .End()
                    .Row(h: 36, px: true, @class: "equipment-slot back picture")
                        .Cell<Picture>()
                    .End()
                    .Row(@class: "spacer").Cell<Layout>().End()
                    .Row(h: 16, px: true, @class: "equipment-slot ring ring-left label")
                        .Cell<Label>(l => l.Text.V = "Lorem ipsum")
                    .End()
                    .Row(h: 36, px: true, @class: "equipment-slot ring ring-left picture")
                        .Cell<Picture>()
                    .End()
                    .Row(@class: "spacer").Cell<Layout>().End()
                    .Row(h: 16, px: true, @class: "equipment-slot ring ring-right label")
                        .Cell<Label>(l => l.Text.V = "Lorem ipsum")
                    .End()
                    .Row(h: 36, px: true, @class: "equipment-slot ring ring-right picture")
                        .Cell<Picture>()
                    .End()
                    .Row(@class: "spacer").Cell<Layout>().End()
                .End()
                .Col(w: 16, px: true, @class: "spacer").Cell<Layout>().End()
            ;
        }

        protected override void OnLayoutRebuilt(Layout oldValue)
        {
            base.OnLayoutRebuilt(oldValue);
        }
        public EquipmentDisplay(GameUI ui) : base(ui)
        {
        }
    }
}
