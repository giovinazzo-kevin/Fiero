﻿using SFML.Graphics;

namespace Fiero.Core
{
    public class Paragraph : UIControl
    {
        private object _lock = new();

        public UIControlProperty<BitmapFont> Font {get; private set;} = new(nameof(Font)) { Propagated = true, Inherited = true };
        public UIControlProperty<Coord> FontSize {get; private set;} = new(nameof(FontSize), new(8, 12), invalidate: true) { Propagated = true, Inherited = true };
        public UIControlProperty<string> Text {get; private set;} = new(nameof(Text), String.Empty, invalidate: true);
        public UIControlProperty<int> Cols {get; private set;} = new(nameof(Cols), 255, invalidate: true);
        public UIControlProperty<int?> Rows {get; private set;} = new(nameof(Rows), null, invalidate: true);
        public UIControlProperty<int?> LineHeight {get; private set;} = new(nameof(LineHeight), null);
        public UIControlProperty<bool> ContentAwareScale {get; private set;} = new(nameof(ContentAwareScale), false, invalidate: true) { Inherited = false };
        public UIControlProperty<bool> CenterContentH {get; private set;} = new(nameof(CenterContentH), false, invalidate: true);
        public UIControlProperty<bool> CenterContentV {get; private set;} = new(nameof(CenterContentV), false, invalidate: true);
        public UIControlProperty<bool> WrapContent {get; private set;} = new(nameof(WrapContent), true, invalidate: true);

        public int CalculatedRows => Rows.V ?? Labels.Count;
        public int CalculatedLineHeight => LineHeight.V ?? FontSize.V.Y;

        protected readonly List<Label> Labels = new();

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
            var lineHeight = CalculatedLineHeight;
            for (int i = Labels.Count - 1; i >= 0; i--)
            {
                Labels[i].Size.V = new(ContentRenderSize.X, lineHeight);
            }
        }

        protected virtual void OnPositionInvalidated()
        {
            var lineHeight = CalculatedLineHeight;
            var totalHeight = CalculatedRows * lineHeight;
            var h = (int)(ContentRenderSize.Y / 2f - totalHeight / 2f);
            foreach (var (c, i) in Labels.Select((c, i) => (c, i)))
            {
                c.Position.V = new(ContentRenderPos.X, ContentRenderPos.Y + i * lineHeight);
                if (CenterContentV.V)
                {
                    c.Position.V += new Coord(0, h);
                }
            }
        }

        protected virtual void OnMaxLinesInvalidated(int? delta)
        {
            delta ??= CalculatedRows - Labels.Count;
            if (delta < 0)
            {
                for (int i = Children.Count - 1; i >= 0 && delta++ < 0; i--)
                {
                    lock (_lock)
                    {
                        if (Children[i] is not Label label) continue;
                        if (Labels.Contains(label))
                        {
                            label.Text.V = string.Empty;
                            Labels.Remove(label);
                            Children.RemoveAt(i);
                        }
                    }
                }
            }
            else if (delta > 0)
            {
                for (; delta-- > 0;)
                {
                    lock (_lock)
                    {
                        var label = CreateLabel();
                        Children.Add(label);
                        Labels.Add(label);
                    }
                }
                OnTextInvalidated();
            }
            OnPositionInvalidated();
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
            if (Rows.V is null)
            {
                OnMaxLinesInvalidated(lines.Count - Labels.Count);
            }
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
            lock (_lock)
            {
                foreach (var (c, l) in Labels.Zip(lines.Concat(Enumerable.Repeat(string.Empty, CalculatedRows)).Take(CalculatedRows)))
                {
                    var len = Math.Min(l.Length, Cols);
                    c.Text.V = l[..len];
                }
            }
            OnSizeInvalidated();
            OnPositionInvalidated();
            lock (_lock)
            {
                MinimumContentSize = Labels.Aggregate(Coord.Zero, (a, b) => b.MinimumContentSize * Coord.PositiveY + a)
                    + new Coord(Labels.Max(x => x.MinimumContentSize.X), 0)
                    + Padding.V * 2;
            }
        }
    }
}
