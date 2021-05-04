using SFML.Graphics;
using System;
using System.Linq;

namespace Fiero.Core
{
    public class Paragraph : UIControl
    {
        protected readonly Func<string, Text> GetText;

        private string _string;
        public event Action<Paragraph, string> TextChanged;
        public string Text {
            get => _string;
            set {
                var oldValue = _string;
                if (oldValue != value) {
                    TextChanged?.Invoke(this, oldValue);
                    _string = value;
                    Children.RemoveAll(x => x is Label);
                    foreach (var line in _string.Split('\n')) {
                        Children.Add(new Label(Input, MaxLength, GetText) {
                            Scale = Scale,
                            Position = new(Position.X, Position.Y + (int)((Children.Count - 1) * Size.Y / MaxLines * 1.5)),
                            ActiveColor = ActiveColor,
                            InactiveColor = InactiveColor,
                            Text = line,
                        });
                        if (Children.Count > MaxLines) {
                            break;
                        }
                    }
                }
            }
        }
        public readonly int MaxLength, MaxLines;

        public Paragraph(GameInput input, Frame frame, int maxLength, int maxLines, Func<string, Text> getText) : base(input)
        {
            MaxLength = maxLength;
            MaxLines = maxLines;
            GetText = getText;
            Text = String.Empty;
            Children.Add(frame);
        }
    }
}
