using SFML.Graphics;
using System;
using System.Linq;

namespace Fiero.Core
{
    public class Label : UIControl
    {
        protected BitmapText LabelDrawable;

        protected readonly Func<string, BitmapText> GetText;

        public readonly UIControlProperty<uint> FontSize = new(nameof(FontSize), 8) { Propagated = true };
        public readonly UIControlProperty<string> Text = new(nameof(Text), String.Empty, invalidate: true);
        public readonly UIControlProperty<int> MaxLength = new(nameof(MaxLength), 255);
        public readonly UIControlProperty<bool> ContentAwareScale = new(nameof(ContentAwareScale), false);
        public readonly UIControlProperty<bool> CenterContentH = new(nameof(CenterContentH), false);
        public readonly UIControlProperty<bool> CenterContentV = new(nameof(CenterContentV), true);

        public string DisplayText => String.IsNullOrEmpty(Text.V)
            ? String.Empty
            : String.Join(String.Empty, Text.V.Take(MaxLength));

        protected virtual void OnTextInvalidated()
        {
            var text = DisplayText;
            if (LabelDrawable is null)
            {
                LabelDrawable = GetText(text);
            }
            if (ContentAwareScale)
            {
                // Correct for error and warp to fit by rescaling
                LabelDrawable.Scale = Scale * ContentRenderSize.ToVec() / LabelDrawable.GetLocalBounds().Size();
            }
            else
            {
                // Calculate scale as a proportion of font size
                var factor = FontSize.V / (float)LabelDrawable.Font.Size.X;
                LabelDrawable.Text = text;
                LabelDrawable.Scale = Scale.V * factor;
            }
        }

        public Label(GameInput input, Func<string, BitmapText> getText) : base(input)
        {
            GetText = getText;
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
            DrawText(this, LabelDrawable, new(), target, states);
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
                var delta = label.ContentRenderPos - drawablePos - drawableSize / 2f + label.ContentRenderSize / 2f - subDelta / 2;
                var deltaCoord = delta.Round().ToCoord();
                if (label.CenterContentH)
                {
                    text.Position += deltaCoord * new Coord(1, 0);
                }
                if (label.CenterContentV)
                {
                    text.Position += deltaCoord * new Coord(0, 1);
                }
                text.FillColor = label.Foreground;
                target.Draw(text, states);
            }
        }
    }
}
