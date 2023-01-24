using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Core
{
    public class Paragraph : UIControl
    {
        protected readonly Func<string, BitmapText> GetText;

        public readonly UIControlProperty<uint> FontSize = new(nameof(FontSize), 8);
        public readonly UIControlProperty<string> Text = new(nameof(Text), String.Empty);
        public readonly UIControlProperty<int> Cols = new(nameof(Cols), 255);
        public readonly UIControlProperty<int> Rows = new(nameof(Rows), 10);
        public readonly UIControlProperty<bool> ContentAwareScale = new(nameof(ContentAwareScale), false) { Inherited = false };
        public readonly UIControlProperty<bool> CenterContentH = new(nameof(CenterContentH), false);
        public readonly UIControlProperty<bool> CenterContentV = new(nameof(CenterContentV), true);

        protected IEnumerable<Label> Labels => Children.OfType<Label>();

        public Paragraph(GameInput input, Func<string, BitmapText> getText) : base(input)
        {
            GetText = getText;
            Size.ValueChanged += (owner, old) =>
            {
                OnSizeInvalidated();
                OnPositionInvalidated();
            };
            Position.ValueChanged += (owner, old) =>
            {
                OnPositionInvalidated();
            };
            FontSize.ValueChanged += (owner, old) =>
            {
                OnTextInvalidated();
            };
            Text.ValueChanged += (owner, old) =>
            {
                OnTextInvalidated();
            };
            Rows.ValueChanged += (owner, old) =>
            {
                OnMaxLinesInvalidated(Rows - old);
            };
            OnMaxLinesInvalidated(Rows); // also calls OnTextInvalidated()
        }

        protected virtual void OnSizeInvalidated()
        {
            foreach (var (c, i) in Labels.Select((c, i) => (c, i)))
            {
                c.Size.V = new(ContentRenderSize.X, ContentRenderSize.Y / Rows);
            }
        }

        protected virtual void OnPositionInvalidated()
        {
            foreach (var (c, i) in Labels.Select((c, i) => (c, i)))
            {
                c.Position.V = new(ContentRenderPos.X, ContentRenderPos.Y + i * ContentRenderSize.Y / Rows);
            }
        }

        protected virtual void OnMaxLinesInvalidated(int delta)
        {
            if (delta < 0)
            {
                for (int i = Children.Count - 1; i >= 0 && delta++ < 0; i--)
                {
                    if (Children[i] is Label)
                        Children.RemoveAt(i);
                }
            }
            else
            {
                for (; delta-- > 0;)
                {
                    var label = new Label(Input, GetText);
                    label.InheritProperties(this);
                    label.Background.V = Color.Transparent;
                    Children.Add(label);
                }
                OnTextInvalidated();
            }
        }

        protected virtual void OnTextInvalidated()
        {
            foreach (var (c, t) in Labels.Zip(Text.V.Split('\n').TakeLast(Rows)))
            {
                c.Text.V = t;
            }
        }
    }
}
