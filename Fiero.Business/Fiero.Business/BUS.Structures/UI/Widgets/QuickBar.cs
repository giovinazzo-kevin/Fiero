using Fiero.Core;
using System.Linq;

namespace Fiero.Business
{
    [TransientDependency]
    public class QuickBar : Widget
    {
        public readonly GameResources Resources;
        public readonly QuickSlotHelper QuickSlotHelper;

        protected override void DefaultSize() { }

        protected override LayoutStyleBuilder DefineStyles(LayoutStyleBuilder builder) => base.DefineStyles(builder)
            .AddRule<UIControl>(r => r
                .Match(l => l.HasAnyClass("quickbar-slot"))
                .Apply(l =>
                {
                    l.OutlineThickness.V = 2;
                    l.OutlineColor.V = UI.GetColor(ColorName.Magenta);
                }))
            ;

        protected override LayoutGrid RenderContent(LayoutGrid grid)
        {
            return grid
                .Repeat(9, (i, grid) => grid
                    .Col(w: 2, px: true, @class: "spacer").End()
                    .Col(w: 36, px: true, @class: "quickbar-slot")
                        .Cell<Picture>()
                        .Cell<Label>(l =>
                        {
                            l.Text.V = $"{(i + 1)}";
                            l.VerticalAlignment.V = VerticalAlignment.Top;
                        })
                        .Cell<Label>(l =>
                        {
                            l.VerticalAlignment.V = VerticalAlignment.Bottom;
                            l.HorizontalAlignment.V = HorizontalAlignment.Left;
                        })
                        .Cell<Label>(l =>
                        {
                            l.VerticalAlignment.V = VerticalAlignment.Top;
                            l.HorizontalAlignment.V = HorizontalAlignment.Right;
                        })
                    .End()
                    .Col(w: 2, px: true, @class: "spacer").End())
            ;
        }

        protected override void OnLayoutRebuilt(Layout oldValue)
        {
            base.OnLayoutRebuilt(oldValue);
            QuickSlotHelper_QuickSlotChanged(QuickSlotHelper);
        }
        public QuickBar(GameUI ui, QuickSlotHelper helper, GameResources resources, ActionSystem action) : base(ui)
        {
            Resources = resources;
            QuickSlotHelper = helper;
            QuickSlotHelper.QuickSlotChanged += QuickSlotHelper_QuickSlotChanged;
            QuickSlotHelper.SlotActivated += (obj, __) => QuickSlotHelper_QuickSlotChanged(obj);
        }

        void QuickSlotHelper_QuickSlotChanged(QuickSlotHelper obj)
        {
            if (Layout is null) return;
            var pictures = Layout.Query<Picture>(l => true, g => g.HasAnyClass("quickbar-slot"))
                .ToArray();
            var labels = Layout.Query<Label>(l => true, g => g.HasAnyClass("quickbar-slot"))
                .ToArray();
            for (int i = 0; i < pictures.Length; i++)
            {
                pictures[i].Sprite.V = null;
                labels[i * 3 + 1].Text.V = string.Empty;
                labels[i * 3 + 2].Text.V = string.Empty;
            }
            foreach (var (i, name, drawable) in obj.GetSlots())
            {
                pictures[i - 1].Sprite.V = Resources.Sprites.Get(drawable.Render.Texture, drawable.Render.Sprite, drawable.Render.Color);
                if (drawable is Consumable c)
                {
                    labels[(i - 1) * 3 + 1].Text.V = $"{c.ConsumableProperties.RemainingUses}";
                }
                labels[(i - 1) * 3 + 2].Text.V = string.IsNullOrEmpty(name) ? string.Empty : name.First().ToString();
            }
        }
    }
}
