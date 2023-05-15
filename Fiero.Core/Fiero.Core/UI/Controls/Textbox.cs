using SFML.Graphics;
using SFML.Window;
using System;
using System.Diagnostics;
using System.Text;

namespace Fiero.Core
{
    public class Textbox : Label
    {
        public const int CaretBlinkIntervalMs = 500;
        private readonly Stopwatch _caretStopwatch = new();
        private volatile bool _caretShown = false;
        private BitmapText _caret;

        public event Action<Textbox> EnterPressed;

        public Textbox(GameInput input) : base(input)
        {
            IsInteractive.V = true;
            IsActive.ValueChanged += (_, old) =>
            {
                if (IsActive.V)
                    Input.TryStealFocus(this);
                else
                    Input.TryRestoreFocus(this);
            };
            _caretStopwatch.Start();
        }

        protected bool IsPrintable(Keyboard.Key k, out char representation)
        {
            const string symbols = ")!@#$%^&*(";

            var i = (int)k;
            var shift = Input.IsKeyDown(Keyboard.Key.LShift) || Input.IsKeyDown(Keyboard.Key.RShift);

            if (i >= (int)Keyboard.Key.A && i <= (int)Keyboard.Key.Z)
            {
                representation = shift
                    ? (char)('A' + (i - (int)Keyboard.Key.A))
                    : (char)('a' + (i - (int)Keyboard.Key.A));
                return true;
            }
            if (i >= (int)Keyboard.Key.Num0 && i <= (int)Keyboard.Key.Num9)
            {
                representation = shift
                    ? symbols[(i - (int)Keyboard.Key.Num0)]
                    : (char)('0' + (i - (int)Keyboard.Key.Num0));
                return true;
            }

            switch (k)
            {
                case Keyboard.Key.LBracket: representation = '['; return true;
                case Keyboard.Key.RBracket: representation = ']'; return true;
                case Keyboard.Key.Backslash: representation = '\\'; return true;
                case Keyboard.Key.Slash: representation = shift ? '?' : '/'; return true;
                case Keyboard.Key.Semicolon: representation = shift ? ':' : ';'; return true;
                case Keyboard.Key.Hyphen: representation = shift ? '_' : '-'; return true;
                case Keyboard.Key.Period: representation = shift ? '>' : '.'; return true;
                case Keyboard.Key.Comma: representation = shift ? '<' : ','; return true;
                case Keyboard.Key.Equal: representation = shift ? '+' : '='; return true;
                case Keyboard.Key.Tilde: representation = shift ? '~' : '`'; return true;
                case Keyboard.Key.Quote: representation = shift ? '"' : '\''; return true;
                case Keyboard.Key.Add: representation = '+'; return true;
                case Keyboard.Key.Subtract: representation = '-'; return true;
                case Keyboard.Key.Multiply: representation = '*'; return true;
                case Keyboard.Key.Divide: representation = '/'; return true;
                case Keyboard.Key.Space: representation = ' '; return true;
            }

            representation = default;
            return false;
        }

        public override void Update()
        {
            base.Update();
            if (!IsActive)
            {
                _caretShown = false;
                _caret = null;
                _caretStopwatch.Restart();
                return;
            }
            var text = new StringBuilder(Text);
            bool enterPressed = false;
            foreach (var key in Input.KeysPressed())
            {
                if (IsPrintable(key, out var ch) && text.Length < MaxLength.V)
                {
                    text.Append(ch);
                }
                else
                {
                    switch (key)
                    {
                        case Keyboard.Key.Backspace:
                            if (text.Length > 0)
                            {
                                text.Remove(text.Length - 1, 1);
                            }
                            break;
                        case Keyboard.Key.Enter:
                            enterPressed = true;
                            break;
                        case Keyboard.Key.Escape:
                        case Keyboard.Key.Tab:
                            // These keys are reserved
                            break;
                    }
                }
            }
            if (_caretStopwatch.ElapsedMilliseconds > 2 * CaretBlinkIntervalMs)
            {
                _caretShown = false;
                _caret = null;
                _caretStopwatch.Restart();
            }
            else if (!_caretShown && _caretStopwatch.ElapsedMilliseconds > CaretBlinkIntervalMs)
            {
                _caret = new BitmapText(Font.V, "|");
                _caretShown = true;
            }
            Text.V = text.ToString();
            if (enterPressed)
                EnterPressed?.Invoke(this);
        }

        public override void Draw(RenderTarget target, RenderStates states)
        {
            if (IsHidden)
                return;
            base.Draw(target, states);
            if (LabelDrawable != null && _caretShown)
            {
                var drawableSize = LabelDrawable.GetLocalBounds().Size();
                DrawText(this, _caret, new(drawableSize.X, 0), target, states);
            }
        }
    }
}
