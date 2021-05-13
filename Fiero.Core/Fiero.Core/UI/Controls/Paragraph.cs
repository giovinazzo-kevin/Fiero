using SFML.Graphics;
using System;
using System.Linq;

namespace Fiero.Core
{
    public class Paragraph : UIControl
    {
        protected readonly Func<string, int, Text> GetText;


        public readonly UIControlProperty<uint> FontSize = new(nameof(FontSize), 8);
        public readonly UIControlProperty<string> Text = new(nameof(Text), String.Empty);
        public readonly UIControlProperty<int> MaxLength = new(nameof(MaxLength), 255);
        public readonly UIControlProperty<int> MaxLines = new(nameof(MaxLines), 16);
        public readonly UIControlProperty<bool> ContentAwareScale = new(nameof(ContentAwareScale), false);
        public readonly UIControlProperty<bool> CenterContent = new(nameof(CenterContent), true);

        public Paragraph(GameInput input, Func<string, int, Text> getText) : base(input)
        {
            GetText = getText;
            Size.ValueChanged += (owner, old) => {
                OnTextInvalidated();
            };
            FontSize.ValueChanged += (owner, old) => {
                OnTextInvalidated();
            };
            Text.ValueChanged += (owner, old) => {
                OnTextInvalidated();
            };
        }

        protected virtual void OnTextInvalidated()
        {
            var text = Text.V;
            var lines =
                ContentAwareScale ? Children.OfType<Label>().Count()
                                  : MaxLines;
            Children.RemoveAll(x => x is Label);
            foreach (var line in text.Split('\n')) {
                var label = new Label(Input, GetText);
                label.Scale.V = Scale.V;
                label.Position.V = new(Position.V.X, Position.V.Y + (int)(Children.Count * Size.V.Y / lines));
                label.MaxLength.V = MaxLength.V;
                label.Foreground.V = Foreground.V;
                label.ContentAwareScale.V = ContentAwareScale.V;
                label.CenterContent.V = CenterContent.V;
                label.Text.V = line;
                label.FontSize.V = FontSize.V;
                label.Size.V = new(Size.V.X, (Size.V.Y / lines));
                Children.Add(label);
                if (Children.Count > MaxLines) {
                    break;
                }
            }
        }
    }
}
