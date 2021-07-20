using SFML.Graphics;
using System;

namespace Fiero.Core
{

    public class Picture<TTexture> : UIControl
        where TTexture : struct, Enum
    {
        protected readonly Func<TTexture, string, Sprite> GetSprite;

        public readonly UIControlProperty<TTexture> TextureName = new(nameof(TextureName));
        public readonly UIControlProperty<string> SpriteName = new(nameof(SpriteName));
        public readonly UIControlProperty<HorizontalAlignment> HorizontalAlignment = new(nameof(HorizontalAlignment));
        public readonly UIControlProperty<bool> LockAspectRatio = new(nameof(LockAspectRatio), true);
        public Sprite Sprite { get; private set; }

        public Picture(GameInput input, Func<TTexture, string, Sprite> getSprite)
            : base(input)
        {
            GetSprite = getSprite;
            SpriteName.ValueChanged += (owner, old) => {
                UpdateSprite();
            };
            TextureName.ValueChanged += (owner, old) => {
                UpdateSprite();
            };

            void UpdateSprite()
            {
                Sprite?.Dispose();
                if (SpriteName.V != null) {
                    Sprite = GetSprite(TextureName.V, SpriteName.V);
                }
            }
        }

        public override void Draw(RenderTarget target, RenderStates states)
        {
            base.Draw(target, states);
            if (Sprite is null)
                return;
            var recSize = new Vec(Sprite.TextureRect.Width, Sprite.TextureRect.Height);
            var aspectRatio = Sprite.TextureRect.Height / (float)Sprite.TextureRect.Width;
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

            Sprite.Scale = spriteSize / recSize;
            if (HorizontalAlignment.V == Core.HorizontalAlignment.Center) {
                Sprite.Position = ContentRenderPos + (ContentRenderSize / 2 - spriteSize / 2);
            }
            else if (HorizontalAlignment.V == Core.HorizontalAlignment.Right) {
                Sprite.Position = ContentRenderPos + (ContentRenderSize - spriteSize);
            }
            else {
                Sprite.Position = ContentRenderPos;
            }
            target.Draw(Sprite, states);
        }
    }
}
