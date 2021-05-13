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
        public readonly UIControlProperty<bool> Center = new(nameof(Center), true);
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
                if (SpriteName.V != null) {
                    Sprite = GetSprite(TextureName.V, SpriteName.V);
                }
            }
        }

        public override void Draw(RenderTarget target, RenderStates states)
        {
            if (Sprite is null)
                return;
            var recSize = new Vec(Sprite.TextureRect.Width, Sprite.TextureRect.Height);
            var aspectRatio = Sprite.TextureRect.Height / (float)Sprite.TextureRect.Width;
            var spriteSize = Size.V.ToVec() * Scale;
            if (LockAspectRatio.V) {
                var newY = spriteSize.X * aspectRatio;
                spriteSize = new Vec(spriteSize.X, newY);
            }

            var sizeRatio = spriteSize / Size.V;
            while(sizeRatio.X > 1 || sizeRatio.Y > 1) {
                spriteSize *= 0.9f;
                sizeRatio = spriteSize / Size.V;
            }

            Sprite.Scale = spriteSize / recSize;
            if (Center.V) {
                Sprite.Position = Position.V + (Size.V / 2 - spriteSize / 2);
            }
            target.Draw(Sprite, states);
        }
    }
}
