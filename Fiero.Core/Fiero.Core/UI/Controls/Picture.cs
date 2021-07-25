using SFML.Graphics;
using System;
using System.Reflection.Metadata.Ecma335;

namespace Fiero.Core
{

    public class Picture : UIControl
    {
        public readonly UIControlProperty<HorizontalAlignment> HorizontalAlignment = new(nameof(HorizontalAlignment));
        public readonly UIControlProperty<bool> LockAspectRatio = new(nameof(LockAspectRatio), true);
        public readonly UIControlProperty<Sprite> Sprite = new(nameof(Sprite), null);


        public Picture(GameInput input)
            : base(input)
        {
        }

        public override void Draw(RenderTarget target, RenderStates states)
        {
            base.Draw(target, states);
            if(!(Sprite is { } spriteDef)) {
                return;
            }
            using var sprite = new Sprite(spriteDef);
            var recSize = new Vec(sprite.TextureRect.Width, sprite.TextureRect.Height);
            var aspectRatio = sprite.TextureRect.Height / (float)sprite.TextureRect.Width;
            var spriteSize = ContentRenderSize.ToVec() * Scale;
            if (LockAspectRatio.V) {
                var newY = spriteSize.X * aspectRatio;
                spriteSize = new Vec(spriteSize.X, newY);
            }

            var sizeRatio = spriteSize / ContentRenderSize;
            while(sizeRatio.X > Scale.V.X || sizeRatio.Y > Scale.V.Y) {
                spriteSize *= 0.9f;
                sizeRatio = spriteSize / ContentRenderSize;
            }

            sprite.Scale = spriteSize / recSize;
            if (HorizontalAlignment.V == Core.HorizontalAlignment.Center) {
                sprite.Position = ContentRenderPos + (ContentRenderSize / 2 - spriteSize / 2);
            }
            else if (HorizontalAlignment.V == Core.HorizontalAlignment.Right) {
                sprite.Position = ContentRenderPos + (ContentRenderSize - spriteSize);
            }
            else {
                sprite.Position = ContentRenderPos;
            }
            target.Draw(sprite, states);
        }
    }
}
