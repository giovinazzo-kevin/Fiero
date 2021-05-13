using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Core
{
    public class Label : UIControl
    {
        private Text _drawable;
        private double _knownTextSize;

        protected readonly Func<string, int, Text> GetText;

        public readonly UIControlProperty<uint> FontSize = new(nameof(FontSize), 8) { Propagate = true };
        public readonly UIControlProperty<string> Text = new(nameof(Text), String.Empty);
        public readonly UIControlProperty<int> MaxLength = new(nameof(MaxLength), 255);
        public readonly UIControlProperty<bool> ContentAwareScale = new(nameof(ContentAwareScale), false);
        public readonly UIControlProperty<bool> CenterContentH = new(nameof(CenterContentH), true);
        public readonly UIControlProperty<bool> CenterContentV = new(nameof(CenterContentV), true);

        public string DisplayText => String.Join(String.Empty, (Text ?? String.Empty).Take(MaxLength));

        protected virtual void OnTextInvalidated()
        {
            var text = DisplayText;
            if (_drawable is null) {
                _drawable = GetText(text, (int)FontSize.V);
            }
            if (ContentAwareScale) {
                const int testFontSize = 8;
                if(_drawable.DisplayedString != text) {
                    // Calculate font size by extrapolating from a known size
                    _drawable.DisplayedString = text;
                    _drawable.CharacterSize = testFontSize;
                    _knownTextSize = _drawable.GetLocalBounds().Size().Magnitude();
                }
                FontSize.V = (uint)(Size.V.ToVec().Magnitude() * testFontSize / _knownTextSize);
                FontSize.V -= FontSize.V % 4;
                if(FontSize.V <= 4) {
                    FontSize.V = 4;
                }
                // TODO: Figure out why this line causes a memory leak
                _drawable.CharacterSize = FontSize.V;
                // Correct for error and warp to fit by rescaling
                _drawable.Scale = Scale * Size.V.ToVec() / _drawable.GetLocalBounds().Size();
            }
            else {
                _drawable.CharacterSize = FontSize.V;
                _drawable.DisplayedString = text;
                _drawable.Scale = Scale.V;
            }
        }

        public Label(GameInput input, Func<string, int, Text> getText) : base(input)
        {
            GetText = getText;
            Text.ValueChanged += (owner, old) => {
                OnTextInvalidated();
            };
            Size.ValueChanged += (owner, old) => {
                OnTextInvalidated();
            };
            FontSize.ValueChanged += (owner, old) => {
                OnTextInvalidated();
            };
        }

        public override void Draw(RenderTarget target, RenderStates states)
        {
            if (IsHidden)
                return;
            base.Draw(target, states);
            if (_drawable != null) {
                _drawable.Position = Position.V;
                var drawableBounds = _drawable.GetGlobalBounds();
                var drawablePos = drawableBounds.Position();
                var drawableSize = drawableBounds.Size();
                var delta = (Position.V - drawablePos) - drawableSize / 2 + Size.V / 2;
                if (CenterContentH) {
                    _drawable.Position += delta * new Vec(1, 0);
                }
                if (CenterContentV) {
                    _drawable.Position += delta * new Vec(0, 1);
                }
                _drawable.FillColor = Foreground;
                target.Draw(_drawable, states);
            }
        }
    }
}
