using Fiero.Core;
using SFML.Graphics;
using System;
using System.Globalization;
using System.Linq;
using static SFML.Window.Keyboard;

namespace Fiero.Business
{

    /// <summary>
    /// A cell that stores a single byte and represents it as a hexadecimal number that can be typed in and shifted by the user.
    /// </summary>
    public class TrackerHexCell : Label
    {
        private int _textIndex;

        protected readonly Key[] Keys = new[] {
            Key.A, Key.B, Key.C, Key.D, Key.E, Key.F,
            Key.Num0,Key.Num1,Key.Num2,Key.Num3,Key.Num4,Key.Num5,Key.Num6,Key.Num7,Key.Num8,Key.Num9,
            Key.Numpad0,Key.Numpad1,Key.Numpad2,Key.Numpad3,Key.Numpad4,Key.Numpad5,Key.Numpad6,Key.Numpad7,Key.Numpad8,Key.Numpad9,
        };

        public UIControlProperty<byte> MaxValue { get; private set; } = new(nameof(MaxValue), 255);
        public UIControlProperty<byte> MinValue { get; private set; } = new(nameof(MinValue), 0);
        public UIControlProperty<byte> Value { get; private set; } = new(nameof(Value));

        public TrackerHexCell(GameInput input, Func<string, int, Text> getText) 
            : base(input, getText)
        {
            IsInteractive.V = true;
            Value.ValueChanged += (_, __) => {
                var clamped = Math.Clamp(Value, MinValue, MaxValue);
                if(Value.V != clamped) {
                    Value.V = clamped;
                }
                else {
                    Text.V = Value.V.ToString("X2");
                }
            };
            Text.ValueChanged += (_, __) => {
                if(!String.Equals(Text.V, Value.V.ToString("X2"))) {
                    Value.V = Byte.TryParse(Text.V, NumberStyles.HexNumber, null, out var asByte)
                        ? asByte
                        : throw new FormatException("The provided text is not a hex-encoded byte");
                }
            };
            IsActive.ValueChanged += (_, __) => {
                if(!IsActive.V) {
                    _textIndex = 0;
                }
            };
        }

        public override void Update()
        {
            base.Update();
            if(!IsActive) {
                return;
            }
            var interestingKeystrokes = Input.KeysPressed()
                .Intersect(Keys);
            foreach (var k in interestingKeystrokes) {
                var hexLetter = k.ToString().Replace("Numpad", "").Replace("Num", "")[0];
                if(Text.V.Length < 2) {
                    Text.V = $"{hexLetter}{(Text.V.Length == 0 ? '0' : Text.V[0])}";
                }
                else {
                    Text.V = $"{Text.V[_textIndex]}{hexLetter}";
                }
                _textIndex = (_textIndex + 1) % 2;
            }
            if (Input.IsKeyPressed(Key.Up)) {
                Value.V += 1;
            }
            if (Input.IsKeyPressed(Key.Down)) {
                Value.V -= 1;
            }
        }
    }
}
