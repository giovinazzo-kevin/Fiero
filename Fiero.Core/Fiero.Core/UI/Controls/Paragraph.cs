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
        public readonly UIControlProperty<bool> WrapContent = new(nameof(WrapContent), true);

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
                    Children.Add(CreateLabel());
                }
                OnTextInvalidated();
            }
        }

        protected Label CreateLabel()
        {
            var label = new Label(Input);
            label.InheritProperties(this);
            label.Background.V = Color.Transparent;
            return label;
        }

        protected virtual void OnTextInvalidated()
        {
            var lines = Text.V.Split('\n').ToList();
            if (WrapContent)
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].Length <= Cols) continue;
                    var rest = lines[i];
                    var newLines = new List<string>();
                    do
                    {
                        var len = Math.Min(rest.Length, Cols);
                        newLines.Add(rest[..len]);
                        rest = rest.Remove(0, len);
                    }
                    while (rest.Length > 0);
                    lines[i] = newLines[0];
                    lines.InsertRange(i + 1, newLines.Skip(1));
                }
            }
            foreach (var (c, l) in Labels.Zip(lines.Take(Rows)))
            {
                var len = Math.Min(l.Length, Cols);
                c.Text.V = l[..len];
            }
            OnSizeInvalidated();
            OnPositionInvalidated();
        }
    }
}
