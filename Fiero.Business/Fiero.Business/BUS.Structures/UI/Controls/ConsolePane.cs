using Ergo.Lang.Extensions;
using Fiero.Business.Utils;
using Fiero.Core;
using SFML.Graphics;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Fiero.Business
{
    public class ConsolePane : UIControl
    {
        public readonly UIControlProperty<Coord> Cursor = new(nameof(Cursor));
        public readonly UIControlProperty<int> TabSize = new(nameof(TabSize), 4);
        public readonly ObservableCollection<StringBuilder> Lines = new();
        public readonly UIControlProperty<BitmapFont> Font = new(nameof(Font)) { Propagated = true, Inherited = true };
        public readonly UIControlProperty<Coord> FontSize = new(nameof(FontSize), new(8, 12)) { Propagated = true, Inherited = true };

        public Paragraph Paragraph { get; private set; }

        private readonly DelayedDebounce _debounce = new(TimeSpan.FromMilliseconds(20), 1) { Enabled = true };

        public ConsolePane(GameInput input) : base(input)
        {
            Paragraph = new(input);
            Children.Add(Paragraph);
            Size.ValueChanged += Size_ValueChanged;
            Invalidated += _ =>
            {
                Paragraph.InheritProperties(this);
                Paragraph.Background.V = Color.Transparent;
                Paragraph.OutlineThickness.V = 0;
            };
        }

        public void Put(char c)
        {
            while (Lines.Count <= Cursor.V.Y)
                Lines.Add(new StringBuilder());
            var stringBuilder = Lines[Cursor.V.Y];
            switch (c)
            {
                case '\n':
                    Lines.Add(new StringBuilder());
                    Cursor.V += Coord.PositiveY;
                    Cursor.V *= Coord.PositiveY;
                    break;
                case '\b' when stringBuilder.Length > 0 && Cursor.V.X > 0:
                    Cursor.V -= Coord.PositiveX;
                    stringBuilder.Remove(Cursor.V.X, 1);
                    break;
                case '\r':
                    Cursor.V *= Coord.PositiveY;
                    break;
                case '\t':
                    int nextTabStop = ((Cursor.V.X / TabSize) + 1) * TabSize;
                    while (Cursor.V.X < nextTabStop)
                        Put(' ');
                    break;
                default:
                    if (Cursor.V.X <= stringBuilder.Length)
                    {
                        stringBuilder.Insert(Cursor.V.X, c);
                        Cursor.V += Coord.PositiveX;
                    }
                    break;
            }
            OnTextInvalidated();
        }
        public void Write(string s)
        {
            foreach (var c in s)
                Put(c);
        }

        public void WriteLine(string s) => Write(s + Environment.NewLine);

        protected void OnTextInvalidated()
        {
            if (!_debounce.IsDebouncing)
                _debounce.Fire += _debounce_Fire;
            _debounce.Hit();
            void _debounce_Fire(Debounce obj)
            {
                _debounce.Fire -= _debounce_Fire;
                Paragraph.Text.V = Lines.TakeLast(Paragraph.Rows.V).Join("\n");
            }
        }

        private void Size_ValueChanged(UIControlProperty<Coord> p, Coord old)
        {
            var newGrid = Size.V / FontSize.V;
            Paragraph.Rows.V = newGrid.Y;
            Paragraph.Cols.V = newGrid.X;
        }
    }
}
