using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Core
{
    public class Label : UIControl
    {
        protected readonly Func<string, Text> GetText;

        private Text _drawable;
        private string _string;
        private uint _fontSize;
        public event Action<Label, string> TextChanged;
        public string Text {
            get => _string;
            set {
                var oldValue = _string;
                if(oldValue != value) {
                    _drawable = GetText(_string = String.Join("", (value ?? "").Take(MaxLength)));
                    TextChanged?.Invoke(this, oldValue);
                    var fontSize = (uint)Math.Sqrt(Math.Pow(Scale.X * _drawable.CharacterSize, 2) + Math.Pow(Scale.Y * _drawable.CharacterSize, 2));
                    _fontSize = Math.Clamp(fontSize - fontSize % _drawable.CharacterSize, 8, 32);
                }
            }
        }

        public readonly int MaxLength;

        public Label(GameInput input, int maxLength, Func<string, Text> getText) : base(input)
        {
            MaxLength = maxLength;
            GetText = getText;
            Text = String.Empty;
        }

        public override void Draw(RenderTarget target, RenderStates states)
        {
            if (IsHidden)
                return;
            base.Draw(target, states);
            if(_drawable != null) {
                _drawable.Position = new(Position.X * Scale.X, Position.Y * Scale.Y);
                _drawable.FillColor = IsActive ? ActiveColor : InactiveColor;
                _drawable.CharacterSize = _fontSize;
                target.Draw(_drawable, states);
            }
        }
    }
}
