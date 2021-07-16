using SFML.Graphics;
using System;
using System.Linq;

namespace Fiero.Core
{
    public class Paragraph : UIControl
    {
        protected readonly Func<string, int, Text> GetText;


        public readonly UIControlProperty<Action<Text>> ConfigureText = new(nameof(ConfigureText), null);
        public readonly UIControlProperty<uint> FontSize = new(nameof(FontSize), 8);
        public readonly UIControlProperty<string> Text = new(nameof(Text), String.Empty);
        public readonly UIControlProperty<int> MaxLength = new(nameof(MaxLength), 255);
        public readonly UIControlProperty<int> MaxLines = new(nameof(MaxLines), 16);
        public readonly UIControlProperty<bool> ContentAwareScale = new(nameof(ContentAwareScale), false);
        public readonly UIControlProperty<bool> CenterContentH = new(nameof(CenterContentH), false);
        public readonly UIControlProperty<bool> CenterContentV = new(nameof(CenterContentV), true);

        public Paragraph(GameInput input, Func<string, int, Text> getText) : base(input)
        {
            GetText = (s, i) => {
                var text = getText(s, i);
                ConfigureText.V?.Invoke(text);
                return text;
            };
            Size.ValueChanged += (owner, old) => {
                OnTextInvalidated();
            };
            Position.ValueChanged += (owner, old) => {
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
                label.CopyProperties(this);
                label.Background.V = Color.Transparent;
                label.Position.V = new(ContentRenderPos.X, ContentRenderPos.Y + Children.Count * ContentRenderSize.Y / lines);
                label.Text.V = line;
                label.Size.V = new(ContentRenderSize.X, ContentRenderSize.Y / lines);
                Children.Add(label);
                if (Children.Count > MaxLines) {
                    break;
                }
            }
        }
    }
}
