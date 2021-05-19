using SFML.Graphics;
using SFML.Window;
using System;
using System.Linq;
using System.Text;

namespace Fiero.Core
{
    public class Textbox : Label
    {
        public Textbox(GameInput input, Func<string, int, Text> getText) : base(input, getText)
        {
            IsInteractive.V = true;
        }

        protected bool IsPrintable(Keyboard.Key k, out char representation)
        {
            const string symbols = ")!@#$%^&*(";

            var i = (int)k;
            var shift = Input.IsKeyDown(Keyboard.Key.LShift) || Input.IsKeyDown(Keyboard.Key.RShift);

            if (i >= (int)Keyboard.Key.A && i <= (int)Keyboard.Key.Z) {
                representation = shift 
                    ? (char)('A' + (i - (int)Keyboard.Key.A)) 
                    : (char)('a' + (i - (int)Keyboard.Key.A));
                return true;
            }
            if (i >= (int)Keyboard.Key.Num0 && i <= (int)Keyboard.Key.Num9) {
                representation = shift 
                    ? symbols[(i - (int)Keyboard.Key.Num0)] 
                    : (char)('0' + (i - (int)Keyboard.Key.Num0));
                return true;
            }

            switch(k) {
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

        public override void Update(float t, float dt)
        {
            var text = new StringBuilder(Text);
            foreach (var key in Input.KeysPressed()) {
                if(IsPrintable(key, out var ch)) {
                    text.Append(ch);
                }
                else {
                    switch(key) {
                        case Keyboard.Key.Backspace:
                            if(text.Length > 0) {
                                text.Remove(text.Length - 1, 1);
                            }
                            break;
                        case Keyboard.Key.Enter:
                        case Keyboard.Key.Escape:
                        case Keyboard.Key.Tab:
                            // These keys are reserved
                            break;
                    }
                }
            }
            Text.V = text.ToString();
        }
    }
}
