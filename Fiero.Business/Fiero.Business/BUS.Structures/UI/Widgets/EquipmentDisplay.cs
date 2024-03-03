namespace Fiero.Business
{
    [TransientDependency]
    public class EquipmentDisplay : Widget
    {
        public readonly GameResources Resources;
        public readonly UIControlProperty<Actor> Following = new(nameof(Following), null);

        protected override LayoutThemeBuilder DefineStyles(LayoutThemeBuilder builder) => base.DefineStyles(builder)
            .Rule<UIControl>(c => c.Apply(l =>
            {
                l.Background.V = UI.GetColor(ColorName.UIBackground);
            }))
            .Rule<Picture>(r => r
                .Match(l => l.HasAnyClass("equipment-slot"))
                .Apply(l =>
                {
                    l.OutlineThickness.V = 2;
                    l.OutlineColor.V = UI.GetColor(ColorName.Cyan);
                }))
            .Rule<Label>(r => r
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
                        .Cell<Label>()
                    .End()
                    .Row(h: 36, px: true, @class: "equipment-slot head picture")
                        .Cell<Picture>()
                    .End()
                    .Row(@class: "spacer").Cell<Layout>().End()
                    .Row(h: 16, px: true, @class: "equipment-slot torso label")
                        .Cell<Label>()
                    .End()
                    .Row(h: 36, px: true, @class: "equipment-slot torso picture")
                        .Cell<Picture>()
                    .End()
                    .Row(@class: "spacer").Cell<Layout>().End()
                    .Row(h: 16, px: true, @class: "equipment-slot hands label")
                        .Cell<Label>()
                    .End()
                    .Row(h: 36, px: true, @class: "equipment-slot hands picture")
                        .Cell<Picture>()
                    .End()
                    .Row(@class: "spacer").Cell<Layout>().End()
                    .Row(h: 16, px: true, @class: "equipment-slot feet label")
                        .Cell<Label>()
                    .End()
                    .Row(h: 36, px: true, @class: "equipment-slot feet picture")
                        .Cell<Picture>()
                    .End()
                    .Row(@class: "spacer").Cell<Layout>().End()
                    .Row(h: 16, px: true, @class: "equipment-slot hand hand-left label")
                        .Cell<Label>()
                    .End()
                    .Row(h: 36, px: true, @class: "equipment-slot hand hand-left picture")
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
                        .Cell<Label>()
                    .End()
                    .Row(h: 36, px: true, @class: "equipment-slot neck picture")
                        .Cell<Picture>()
                    .End()
                    .Row(@class: "spacer").Cell<Layout>().End()
                    .Row(h: 16, px: true, @class: "equipment-slot back label")
                        .Cell<Label>()
                    .End()
                    .Row(h: 36, px: true, @class: "equipment-slot back picture")
                        .Cell<Picture>()
                    .End()
                    .Row(@class: "spacer").Cell<Layout>().End()
                    .Row(h: 16, px: true, @class: "equipment-slot ring ring-left label")
                        .Cell<Label>()
                    .End()
                    .Row(h: 36, px: true, @class: "equipment-slot ring ring-left picture")
                        .Cell<Picture>()
                    .End()
                    .Row(@class: "spacer").Cell<Layout>().End()
                    .Row(h: 16, px: true, @class: "equipment-slot ring ring-right label")
                        .Cell<Label>()
                    .End()
                    .Row(h: 36, px: true, @class: "equipment-slot ring ring-right picture")
                        .Cell<Picture>()
                    .End()
                    .Row(@class: "spacer").Cell<Layout>().End()
                    .Row(h: 16, px: true, @class: "equipment-slot hand hand-right label")
                        .Cell<Label>()
                    .End()
                    .Row(h: 36, px: true, @class: "equipment-slot hand hand-right picture")
                        .Cell<Picture>()
                    .End()
                    .Row(@class: "spacer").Cell<Layout>().End()
                .End()
                .Col(w: 16, px: true, @class: "spacer").Cell<Layout>().End()
            ;
        }

        protected override void DefaultSize() { }

        public EquipmentDisplay(GameUI ui, GameResources resources) : base(ui)
        {
            Resources = resources;
            Following.ValueChanged += Following_ValueChanged;
        }

        private void Following_ValueChanged(UIControlProperty<Actor> arg1, Actor old)
        {
            if (old?.ActorEquipment != null)
                old.ActorEquipment.EquipmentChanged -= ActorEquipment_EquipmentChanged;
            if (Following.V?.ActorEquipment != null)
            {
                Following.V.ActorEquipment.EquipmentChanged += ActorEquipment_EquipmentChanged;
                ActorEquipment_EquipmentChanged(Following.V.ActorEquipment);
            }
        }

        private void ActorEquipment_EquipmentChanged(ActorEquipmentComponent obj)
        {
            if (Layout is null) return;
            foreach (var picture in Layout.Query<Picture>(l => true, g => true))
                picture.Sprite.V = null;
            foreach (var picture in Layout.Query<Label>(l => true, g => true))
                picture.Text.V = string.Empty;
            foreach (var (k, v) in obj.EquippedItems)
            {
                switch (k)
                {
                    case EquipmentSlotName.Arms: Update("arms", v); break;
                    case EquipmentSlotName.Legs: Update("legs", v); break;
                    case EquipmentSlotName.Head: Update("head", v); break;
                    case EquipmentSlotName.Torso: Update("torso", v); break;
                    case EquipmentSlotName.Back: Update("back", v); break;
                    case EquipmentSlotName.Neck: Update("neck", v); break;
                    case EquipmentSlotName.LeftRing: Update("ring-left", v); break;
                    case EquipmentSlotName.RightRing: Update("ring-right", v); break;
                    case EquipmentSlotName.LeftHand: Update("hand-left", v); break;
                    case EquipmentSlotName.RightHand: Update("hand-right", v); break;
                }
            }
            void Update(string @class, Equipment v)
            {
                Layout.Query<Label>(l => true, g => g.HasAllClasses(@class, "label")).Single().Text.V
                    = v.ItemProperties.Identified ? v.Info.Name : v.ItemProperties.UnidentifiedName;
                Layout.Query<Picture>(l => true, g => g.HasAllClasses(@class, "picture")).Single().Sprite.V
                    = Resources.Sprites.Get(v.Render.Texture, v.Render.Sprite, v.Render.Color);
            }
        }
    }
}
