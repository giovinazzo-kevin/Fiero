using SFML.Graphics;

namespace Fiero.Business
{
    [TransientDependency]
    public class SpeechBubble(GameUI ui, GameResources res, MetaSystem meta, Actor speaker) : Widget(ui)
    {
        const FontName FONT_NAME = FontName.Light;
        const int SPRITE_SIZE = 8; // px

        private int lengthInTiles;

        public override void Open(string title)
        {
            var font = res.Fonts.Get(FONT_NAME);
            var text = new BitmapText(font, title);
            lengthInTiles = text.GetLocalBounds().Size().X / SPRITE_SIZE;
            base.Open(title);
        }
        protected override void DefaultSize() { }
        protected override LayoutThemeBuilder DefineStyles(LayoutThemeBuilder builder) => base.DefineStyles(builder)
            .Rule<Picture>(r => r
                .Match(l => l.HasAnyClass("speech_bubble-start"))
                .Apply(l =>
                {
                    l.Sprite.V = res.Sprites.Get(TextureName.UI, "speech_bubble-l", ColorName.White);
                }))
            .Rule<Picture>(r => r
                .Match(l => l.HasAnyClass("speech_bubble-middle"))
                .Apply(l =>
                {
                    l.Sprite.V = res.Sprites.Get(TextureName.UI, "speech_bubble-m", ColorName.White);
                }))
            .Rule<Picture>(r => r
                .Match(l => l.HasAnyClass("speech_bubble-end"))
                .Apply(l =>
                {
                    l.Sprite.V = res.Sprites.Get(TextureName.UI, "speech_bubble-r", ColorName.White);
                }))
            .Rule<Label>(r => r
                .Match(l => l.HasAnyClass("speech_bubble-end"))
                .Apply(l =>
                {
                    l.Background.V = Color.Red;
                    l.Font.V = res.Fonts.Get(FONT_NAME);
                    l.Text.V = Title.V;
                }))
            ;
        protected override LayoutGrid RenderContent(LayoutGrid grid)
        {
            return grid
                .Row()
                    .Cell<Label>()
                    .Col(w: 8, px: true, @class: "speech_bubble-start").Cell<Picture>().End()
                    .Repeat(lengthInTiles, (_, g) => g
                        .Col(w: 8, px: true, @class: "speech_bubble-middle").Cell<Picture>().End())
                    .Col(w: 8, px: true, @class: "speech_bubble-end").Cell<Picture>().End()
                .End()
            ;
        }

        public override void Update(TimeSpan t, TimeSpan dt)
        {
            if (speaker.IsInvalid())
            {
                Close(ModalWindowButton.None);
                return;
            }
            var viewport = meta.Get<RenderSystem>().Viewport;
            Layout.Position.V = viewport.WorldToScreenPos(speaker.Position());
        }

        public override void Draw(RenderTarget target, RenderStates states)
        {
            base.Draw(target, states);
        }

    }
}
