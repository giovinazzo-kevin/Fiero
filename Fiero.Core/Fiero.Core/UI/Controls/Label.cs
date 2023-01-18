﻿using SFML.Graphics;
using System;
using System.Linq;

namespace Fiero.Core
{
    public class Label : UIControl
    {
        private BitmapText _drawable;
        private double _knownTextSize;

        protected readonly Func<string, BitmapText> GetText;

        public readonly UIControlProperty<uint> FontSize = new(nameof(FontSize), 8) { Propagated = true };
        public readonly UIControlProperty<string> Text = new(nameof(Text), String.Empty);
        public readonly UIControlProperty<int> MaxLength = new(nameof(MaxLength), 255);
        public readonly UIControlProperty<bool> ContentAwareScale = new(nameof(ContentAwareScale), false);
        public readonly UIControlProperty<bool> CenterContentH = new(nameof(CenterContentH), true);
        public readonly UIControlProperty<bool> CenterContentV = new(nameof(CenterContentV), true);

        public string DisplayText => String.IsNullOrEmpty(Text.V)
            ? String.Empty
            : String.Join(String.Empty, Text.V.Take(MaxLength));

        protected virtual void OnTextInvalidated()
        {
            var text = DisplayText;
            if (_drawable is null)
            {
                _drawable = GetText(text);
            }
            if (ContentAwareScale)
            {
                const int testFontSize = 8;
                if (_drawable.Text != text)
                {
                    // Calculate font size by extrapolating from a known size
                    _drawable.Text = text;
                    _knownTextSize = _drawable.GetLocalBounds().Size().ToVec().Magnitude();
                }
                FontSize.V = (uint)(ContentRenderSize.ToVec().Magnitude() * testFontSize / _knownTextSize);
                FontSize.V -= FontSize.V % 4;
                if (FontSize.V <= 4)
                {
                    FontSize.V = 4;
                }
                // Correct for error and warp to fit by rescaling
                _drawable.Scale = Scale * ContentRenderSize.ToVec() / _drawable.GetLocalBounds().Size();
            }
            else
            {
                _drawable.Text = text;
                _drawable.Scale = Scale.V;
            }
        }

        public Label(GameInput input, Func<string, BitmapText> getText) : base(input)
        {
            GetText = getText;
            Text.ValueChanged += (owner, old) =>
            {
                OnTextInvalidated();
            };
            Size.ValueChanged += (owner, old) =>
            {
                OnTextInvalidated();
            };
            FontSize.ValueChanged += (owner, old) =>
            {
                if (!ContentAwareScale)
                {
                    OnTextInvalidated();
                }
            };
        }

        public override void Draw(RenderTarget target, RenderStates states)
        {
            if (IsHidden)
                return;
            base.Draw(target, states);
            if (_drawable != null)
            {
                _drawable.Position = ContentRenderPos;
                var drawableBounds = _drawable.GetGlobalBounds();
                var drawablePos = drawableBounds.Position();
                var drawableSize = drawableBounds.Size();
                var delta = ContentRenderPos - drawablePos - drawableSize / 2f + ContentRenderSize / 2f;
                var deltaCoord = delta.Round().ToCoord();
                if (CenterContentH)
                {
                    _drawable.Position += deltaCoord * new Coord(1, 0);
                }
                if (CenterContentV)
                {
                    _drawable.Position += deltaCoord * new Coord(0, 1);
                }
                _drawable.FillColor = Foreground;
                target.Draw(_drawable, states);
            }
        }
    }
}
