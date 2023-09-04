using SFML.Graphics;
using System.Diagnostics;

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
            if (KeyboardReader.TryReadChar(out var ch, consume: false))
            {
                CharAvailable?.Invoke(this, ch);
                switch (ch)
                {
                    case '\b' when text.Length > 0:
                        text.Remove(text.Length - 1, 1);
                        break;
                    case '\r':
                        enterPressed = true;
                        break;
                    default:
                        if (!char.IsControl(ch) || Char.IsWhiteSpace(ch))
                            text.Append(ch);
                        break;
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
            Text.V = text.ToString()[..Math.Min(MaxLength.V, text.Length)];
            if (enterPressed)
            {
                EnterPressed?.Invoke(this);
                if (ClearOnEnter)
                    Text.V = string.Empty;
            }
        }

        protected override void Render(RenderTarget target, RenderStates states)
        {
            if (IsHidden)
                return;
            base.Render(target, states);
            if (LabelDrawable != null && _caretShown)
            {
                var drawableSize = LabelDrawable.GetLocalBounds().Size();
                DrawText(this, _caret, new(drawableSize.X, 0), target, states);
            }
        }
    }
}
