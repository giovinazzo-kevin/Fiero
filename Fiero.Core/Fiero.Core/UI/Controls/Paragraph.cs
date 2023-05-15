using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Core
{
    public class Paragraph : UIControl
    {
        public readonly UIControlProperty<BitmapFont> Font = new(nameof(Font)) { Propagated = true, Inherited = true };
        public readonly UIControlProperty<Coord> FontSize = new(nameof(FontSize), new(8, 12)) { Propagated = true, Inherited = true };
        public readonly UIControlProperty<string> Text = new(nameof(Text), String.Empty);
        public readonly UIControlProperty<int> Cols = new(nameof(Cols), 255);
        public readonly UIControlProperty<int> Rows = new(nameof(Rows), 10);
        public readonly UIControlProperty<bool> ContentAwareScale = new(nameof(ContentAwareScale), false) { Inherited = false };
        public readonly UIControlProperty<bool> CenterContentH = new(nameof(CenterContentH), false);
        public readonly UIControlProperty<bool> CenterContentV = new(nameof(CenterContentV), true);

        protected IEnumerable<Label> Labels => Children.OfType<Label>();

        public Paragraph(GameInput input) : base(input)
        {
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
            Cols.ValueChanged += (owner, old) =>
            {
                OnTextInvalidated();
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
                    var label = new Label(Input);
                    label.InheritProperties(this);
                    label.Background.V = Color.Transparent;
                    Children.Add(label);
                }
                OnTextInvalidated();
            }
        }

        protected virtual void OnTextInvalidated()
        {
            foreach (var (c, t) in Labels.Zip(Text.V.Split('\n').TakeLast(Rows).Select(x => string.Join(string.Empty, x.Take(Cols)))))
            {
                c.Text.V = t;
            }
        }
    }
}
