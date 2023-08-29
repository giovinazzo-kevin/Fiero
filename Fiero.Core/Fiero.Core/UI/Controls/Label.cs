using SFML.Graphics;

namespace Fiero.Core
{
    public class Label : UIControl
    {
        protected BitmapText LabelDrawable;

        public readonly UIControlProperty<BitmapFont> Font = new(nameof(Font)) { Propagated = true, Inherited = true };
        public readonly UIControlProperty<Coord> FontSize = new(nameof(FontSize), new(8, 12)) { Propagated = true, Inherited = true };
        public readonly UIControlProperty<string> Text = new(nameof(Text), String.Empty, invalidate: true);
        public readonly UIControlProperty<int> MaxLength = new(nameof(MaxLength), 255);
        public readonly UIControlProperty<bool> ContentAwareScale = new(nameof(ContentAwareScale), false);
        public readonly UIControlProperty<HorizontalAlignment> HorizontalAlignment = new(nameof(HorizontalAlignment), Core.HorizontalAlignment.Left);
        public readonly UIControlProperty<VerticalAlignment> VerticalAlignment = new(nameof(VerticalAlignment), Core.VerticalAlignment.Middle);

        public string DisplayText => String.IsNullOrEmpty(Text.V)
            ? String.Empty
            : String.Join(String.Empty, Text.V.Take(MaxLength));

        protected virtual void OnTextInvalidated()
        {
            if (Font.V == null) return;
            var text = DisplayText;
            if (LabelDrawable is null)
            {
                LabelDrawable = new BitmapText(Font.V, text);
            }
            if (ContentAwareScale)
            {
                // Correct for error and warp to fit by rescaling
                LabelDrawable.Scale = Scale * ContentRenderSize.ToVec() / LabelDrawable.GetLocalBounds().Size();
            }
            else
            {
                // Calculate scale as a proportion of font size
                var factor = FontSize.V / Font.V.Size;
                LabelDrawable.Text = text;
                LabelDrawable.Scale = Scale.V * factor;
            }
        }

        public Label(GameInput input) : base(input)
        {
            Text.ValueChanged += (owner, old) =>
            {
                OnTextInvalidated();
            };
            Size.ValueChanged += (owner, old) =>
            {
                OnTextInvalidated();
            };
            ContentAwareScale.ValueChanged += (owner, old) =>
            {
                OnTextInvalidated();
            };
            FontSize.ValueChanged += (owner, old) =>
            {
                if (!ContentAwareScale)
                {
                    OnTextInvalidated();
                }
            };
        }

        public override void Draw(RenderTarget target, RenderStates states)
        {
            if (IsHidden)
                return;
            base.Draw(target, states);
            DrawText(this, LabelDrawable, Origin.V.ToCoord() + Margin.V + Padding.V / 2, target, states);
        }

        public static void DrawText(Label label, BitmapText text, Coord offset, RenderTarget target, RenderStates states)
        {
            if (text != null)
            {
                text.Position = label.ContentRenderPos + offset;
                var drawableBounds = text.GetGlobalBounds();
                var drawablePos = drawableBounds.Position();
                var drawableSize = drawableBounds.Size();
                var subDelta = drawableSize * text.Scale - drawableSize;
                var delta = text.Position - drawablePos - drawableSize / 2f + label.ContentRenderSize / 2f - subDelta / 2f;
                var deltaCoord = delta.Round().ToCoord();
                switch (label.HorizontalAlignment.V)
                {
                    case Core.HorizontalAlignment.Center:
                        text.Position += deltaCoord * new Coord(1, 0);
                        break;
                    case Core.HorizontalAlignment.Right:
                        text.Position += deltaCoord * new Coord(1, 0) * 2;
                        break;
                }
                switch (label.VerticalAlignment.V)
                {
                    case Core.VerticalAlignment.Middle:
                        text.Position += deltaCoord * new Coord(0, 1);
                        break;
                    case Core.VerticalAlignment.Bottom:
                        text.Position += deltaCoord * new Coord(0, 1) * 2;
                        break;
                }
                text.FillColor = label.Foreground;
                target.Draw(text, states);
            }
        }
    }
}
