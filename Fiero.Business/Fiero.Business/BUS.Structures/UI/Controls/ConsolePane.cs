using Ergo.Lang.Extensions;
using Fiero.Business.Utils;
using System.Collections.ObjectModel;
using System.Text;

namespace Fiero.Business
{
    public class ConsolePane : Paragraph
    {
        public readonly UIControlProperty<Coord> Cursor = new(nameof(Cursor), invalidate: true);
        public readonly UIControlProperty<int> TabSize = new(nameof(TabSize), 4, invalidate: true);
        public readonly ObservableCollection<StringBuilder> Lines = new();
        public readonly ObservableCollection<string> History = new();
        public readonly UIControlProperty<int> HistoryCursor = new(nameof(HistoryCursor), 0, invalidate: true);
        public readonly UIControlProperty<int> Scroll = new(nameof(Scroll), 0, invalidate: true);
        public readonly UIControlProperty<int> ScrollAmount = new(nameof(ScrollAmount), 8, invalidate: true);
        public TextBox Caret { get; private set; }

        private readonly RampingDebounce _writeDebounce = new(
            minCooldown: TimeSpan.FromMilliseconds(10),
            maxCooldown: TimeSpan.FromMilliseconds(50),
            rampUpFactor: TimeSpan.FromMilliseconds(50),
            decayFactor: TimeSpan.FromMilliseconds(1000)
        )
        { Enabled = true };
        public ConsolePane(GameInput input, KeyboardInputReader reader) : base(input)
        {
            Caret = new(input, reader);
            Size.ValueChanged += Size_ValueChanged;
            IsActive.ValueChanged += IsActive_ValueChanged;
            Invalidated += src =>
            {
                Caret.Size.V = Caret.FontSize.V;
                Caret.Padding.V = Caret.Margin.V = Coord.Zero;
                Cursor_ValueChanged(null, Cursor);
                if (src != this && src != null)
                    Invalidate();
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
            var glyphSize = ContentRenderSize / new Vec(Cols.V, CalculatedRows);
            var renderOffset = ContentRenderPos - Position.V;
            var physicalCursorPos = ((Cursor.V - new Coord(0, Scroll.V)) * glyphSize + renderOffset).ToCoord();

            // Check if the cursor would be physically outside of the paragraph
            if (!Contains(physicalCursorPos).Any())
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

        public override void Update(TimeSpan t, TimeSpan dt)
        {
            base.Update(t, dt);
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
            if (Input.IsKeyPressed(VirtualKeys.Left) && Cursor.V.X > 0)
            {
            }
            if (Input.IsKeyPressed(VirtualKeys.Right) && Cursor.V.X < Caret.Text.V.Length - 1)
            {
            }
        }
        public void ScrollDown()
        {
            if (Lines.Count > Rows.V)
            {
                Scroll.V = Math.Min(Scroll.V + ScrollAmount.V, Lines.Count - CalculatedRows);
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
                Scroll.V = Cursor.V.Y - CalculatedRows + 1;
                OnTextInvalidated();
            }
            Caret.IsHidden.V = false;
        }

        public void Put(char c, bool invalidate = true)
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
                        Put(' ', invalidate);
                    break;
                default:
                    if (Cursor.V.X <= stringBuilder.Length)
                    {
                        stringBuilder.Insert(Cursor.V.X, c);
                        Cursor.V += Coord.PositiveX;
                    }
                    break;
            }
            if (invalidate)
            {
                OnTextInvalidated();
                ScrollToCursor();
            }
        }
        public void Write(string s, bool invalidate = true)
        {
            foreach (var c in s)
                Put(c, false);
            if (invalidate)
            {
                OnTextInvalidated();
                ScrollToCursor();
            }
        }

        public void WriteLine(string s, bool invalidate = true) => Write(s + Environment.NewLine, invalidate);

        protected override void OnTextInvalidated()
        {
            base.OnTextInvalidated();
            IsFrozen = true;
            if (!_writeDebounce.Enabled)
                _debounce_Fire(null);
            if (!_writeDebounce.IsDebouncing)
                _writeDebounce.Fire += _debounce_Fire;
            _writeDebounce.Hit();
            void _debounce_Fire(Debounce obj)
            {
                if (obj != null)
                    obj.Fire -= _debounce_Fire;
                IsFrozen = false;
                Text.V = Lines
                    .Skip(Scroll.V)
                    .Take(CalculatedRows)
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
