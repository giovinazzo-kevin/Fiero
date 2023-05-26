using Ergo.Lang.Extensions;
using Fiero.Business.Utils;
using Fiero.Core;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Fiero.Business
{
    public class ConsolePane : Paragraph
    {
        public readonly UIControlProperty<Coord> Cursor = new(nameof(Cursor));
        public readonly UIControlProperty<int> TabSize = new(nameof(TabSize), 4);
        public readonly ObservableCollection<StringBuilder> Lines = new();
        public readonly ObservableCollection<string> History = new();
        public readonly UIControlProperty<int> HistoryCursor = new(nameof(HistoryCursor), 0);
        public readonly UIControlProperty<int> Scroll = new(nameof(Scroll), 0);
        public readonly UIControlProperty<int> ScrollAmount = new(nameof(ScrollAmount), 8);
        public TextBox Caret { get; private set; }

        private readonly Debounce _debounce = new(TimeSpan.FromMilliseconds(10)) { Enabled = true };
        public ConsolePane(GameInput input, KeyboardInputReader reader) : base(input)
        {
            Caret = new(input, reader);
            Size.ValueChanged += Size_ValueChanged;
            IsActive.ValueChanged += IsActive_ValueChanged;
            Invalidated += _ =>
            {
                Caret.Size.V = Caret.FontSize.V;
                Caret.Padding.V = Caret.Margin.V = Coord.Zero;
                Cursor_ValueChanged(null, Cursor);
            };
            History.CollectionChanged += History_CollectionChanged;
            Cursor.ValueChanged += Cursor_ValueChanged;
            Caret.CharAvailable += Caret_CharAvailable;
            Children.Add(Caret);
            IsInteractive.V = true;
            WrapContent.V = false;
            Caret.InheritProperties(this);
            Caret.ClearOnEnter.V = true;
            Caret.IsHidden.V = true;
        }

        private void History_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            HistoryCursor.V = History.Count - 1;
        }

        private void Caret_CharAvailable(TextBox arg1, char arg2)
        {
            ScrollToCursor();
        }

        private void IsActive_ValueChanged(UIControlProperty<bool> arg1, bool arg2)
        {
            Caret.IsActive.V = IsActive.V;
        }
        private void Cursor_ValueChanged(UIControlProperty<Coord> arg1, Coord arg2)
        {
            var glyphSize = ContentRenderSize / new Vec(Cols.V, Rows.V);
            var renderOffset = ContentRenderPos - Position.V;
            var physicalCursorPos = ((Cursor.V - new Coord(0, Scroll.V)) * glyphSize + renderOffset).ToCoord();

            // Check if the cursor would be physically outside of the paragraph
            if (!Contains(physicalCursorPos, out _))
            {
                // If so, hide it
                Caret.IsHidden.V = true;
            }
            else
            {
                // Otherwise, show it and update its position
                Caret.IsHidden.V = false;
                Caret.Position.V = physicalCursorPos;
            }
            // Make sure the cursor is in view while it moves
            if (arg1 != null)
                ScrollToCursor();
        }

        public override void Update()
        {
            base.Update();
            if (Input.IsMouseWheelScrollingDown())
            {
                ScrollDown();
            }
            else if (Input.IsMouseWheelScrollingUp())
            {
                ScrollUp();
            }
            if (Input.IsKeyPressed(VirtualKeys.Up) && History.Any())
            {
                if (HistoryCursor.V < 0)
                    HistoryCursor.V = History.Count - 1;
                Caret.Text.V = History[HistoryCursor.V--];
            }
        }
        public void ScrollDown()
        {
            if (Lines.Count > Rows.V)
            {
                Scroll.V = Math.Min(Scroll.V + ScrollAmount.V, Lines.Count - Rows.V);
                Cursor_ValueChanged(null, Cursor.V);
                OnTextInvalidated();
            }
        }

        public void ScrollUp()
        {
            if (Scroll.V > 0)
            {
                Scroll.V = Math.Max(Scroll.V - ScrollAmount.V, 0);
                Cursor_ValueChanged(null, Cursor.V);
                OnTextInvalidated();
            }
        }
        public void ScrollToCursor()
        {
            // If the cursor is above the visible area
            if (Cursor.V.Y < Scroll.V)
            {
                Scroll.V = Cursor.V.Y;
                OnTextInvalidated();
            }
            // If the cursor is below the visible area
            else if (Cursor.V.Y >= Scroll.V + Rows.V)
            {
                Scroll.V = Cursor.V.Y - Rows.V + 1;
                OnTextInvalidated();
            }
            Caret.IsHidden.V = false;
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
            ScrollToCursor();
        }
        public void Write(string s)
        {
            foreach (var c in s)
                Put(c);
        }

        public void WriteLine(string s) => Write(s + Environment.NewLine);

        protected override void OnTextInvalidated()
        {
            base.OnTextInvalidated();
            if (!_debounce.IsDebouncing)
                _debounce.Fire += _debounce_Fire;
            _debounce.Hit();
            void _debounce_Fire(Debounce obj)
            {
                _debounce.Fire -= _debounce_Fire;
                Text.V = Lines
                    .Skip(Scroll.V)
                    .Take(Rows.V)
                    .Join("\n");
            }
        }

        private void Size_ValueChanged(UIControlProperty<Coord> p, Coord old)
        {
            var newGrid = ContentRenderSize / FontSize.V;
            Rows.V = newGrid.Y;
            Cols.V = newGrid.X;
        }
    }
}
