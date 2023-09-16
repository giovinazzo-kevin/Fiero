using SFML.Graphics;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Fiero.Core
{
    public class TextBox : Label
    {
        public const int CaretBlinkIntervalMs = 500;
        private readonly Stopwatch _caretStopwatch = new();
        private volatile bool _caretShown = false;
        private BitmapText _caret;

        public event Action<TextBox> EnterPressed;
        public event Action<TextBox, VirtualKeys> KeyPressed;
        public event Action<TextBox, char> CharAvailable;

        public readonly KeyboardInputReader KeyboardReader;

        public readonly UIControlProperty<int> CaretPosition = new(nameof(CaretPosition), invalidate: true);
        public readonly UIControlProperty<bool> ClearOnEnter = new(nameof(ClearOnEnter), false);

        public TextBox(GameInput input, KeyboardInputReader inputReader) : base(input)
        {
            KeyboardReader = inputReader;
            IsInteractive.V = true;
            IsActive.ValueChanged += (_, old) =>
            {
                if (IsActive.V)
                    Input.TryStealFocus(this);
                else
                    Input.TryRestoreFocus(this);
            };
            _caretStopwatch.Start();
            Font.ValueChanged += (_, __) =>
            {
                if (Font.V != null)
                    _caret = new BitmapText(Font.V, $"█");
            };
            Text.ValueChanged += (_, __) =>
            {
                if (CaretPosition.V > (Text.V?.Length ?? 0))
                    CaretPosition.V = Text.V?.Length ?? 0;
            };
        }

        public override void Update(TimeSpan t, TimeSpan dt)
        {
            base.Update(t, dt);
            if (!IsActive)
            {
                _caretShown = false;
                _caretStopwatch.Restart();
                return;
            }
            var text = new StringBuilder(Text);
            bool enterPressed = false;
            if (KeyboardReader.Input.IsKeyPressed(VirtualKeys.Left) && CaretPosition.V > 0)
            {
                if (KeyboardReader.Input.IsKeyDown(VirtualKeys.Control))
                {
                    // Move to the previous word boundary
                    var regex = new Regex(@"\w+\W*$");
                    var match = regex.Match(text.ToString().Substring(0, CaretPosition.V));
                    if (match.Success)
                    {
                        CaretPosition.V = match.Index;
                    }
                    else
                    {
                        CaretPosition.V = 0;
                    }
                }
                else
                    CaretPosition.V--;
            }
            if (KeyboardReader.Input.IsKeyPressed(VirtualKeys.Right) && CaretPosition.V < text.Length)
            {
                if (KeyboardReader.Input.IsKeyDown(VirtualKeys.Control) && CaretPosition.V < text.Length)
                {
                    // Move to the next word boundary
                    var regex = new Regex(@"\w+\W*");
                    var match = regex.Match(text.ToString().Substring(CaretPosition.V));
                    if (match.Success)
                    {
                        CaretPosition.V += match.Length;
                    }
                    else
                    {
                        CaretPosition.V = text.Length;
                    }
                }
                else
                    CaretPosition.V++;
            }
            if (KeyboardReader.Input.IsKeyPressed(VirtualKeys.Delete))
            {
                if (CaretPosition.V < text.Length)
                    text.Remove(CaretPosition.V, 1);
            }
            if (KeyboardReader.TryReadChar(out var ch, consume: false))
            {
                CharAvailable?.Invoke(this, ch);
                switch (ch)
                {
                    case '\b' when CaretPosition.V > 0:
                        text.Remove(CaretPosition.V - 1, 1);
                        CaretPosition.V -= 1;
                        break;
                    case '\r':
                        enterPressed = true;
                        break;
                    default:
                        if (!char.IsControl(ch) || Char.IsWhiteSpace(ch))
                        {
                            text.Insert(CaretPosition.V, ch);
                            CaretPosition.V += 1;
                        }
                        break;
                }
                Invalidate();
            }
            if (_caretStopwatch.ElapsedMilliseconds > 2 * CaretBlinkIntervalMs)
            {
                _caretShown = false;
                _caretStopwatch.Restart();
                Invalidate();
            }
            else if (!_caretShown && _caretStopwatch.ElapsedMilliseconds > CaretBlinkIntervalMs)
            {
                _caretShown = true;
                Invalidate();
            }
            Text.V = text.ToString()[..Math.Min(MaxLength.V, text.Length)];
            if (enterPressed)
            {
                EnterPressed?.Invoke(this);
                if (ClearOnEnter)
                {
                    Text.V = string.Empty;
                    CaretPosition.V = 0;
                }
                Invalidate();
            }
        }

        protected override void Repaint(RenderTarget target, RenderStates states)
        {
            if (IsHidden)
                return;
            base.Repaint(target, states);
            if (_caretShown)
            {
                var caretOffset = CaretPosition.V * FontSize.V.X;
                DrawText(this, _caret, new(caretOffset, 0), target, states);
            }
        }
    }
}
