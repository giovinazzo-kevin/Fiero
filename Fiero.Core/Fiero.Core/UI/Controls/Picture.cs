using SFML.Graphics;

namespace Fiero.Core
{

    public class Picture : UIControl
    {
        public UIControlProperty<bool> LockAspectRatio {get; private set;} = new(nameof(LockAspectRatio), true, invalidate: true);
        public UIControlProperty<SpriteDef> Sprite {get; private set;} = new(nameof(Sprite), default, invalidate: true);
        private Sprite _sprite;


        public Picture(GameInput input, GameSprites sprites)
            : base(input)
        {
            this.IsInteractive.V = true;
            Sprite.ValueChanged += (s, e) =>
            {
                _sprite?.Dispose();
                _sprite = null;
                if (Equals(Sprite.V, default))
                    return;
                _sprite = sprites.Get(Sprite.V.Texture, Sprite.V.Name, Sprite.V.Tint, Sprite.V.RngSeed);
            };
        }

        protected override void Repaint(RenderTarget target, RenderStates states)
        {
            base.Repaint(target, states);
            if (_sprite is null)
                return;
            using var sprite = new Sprite(_sprite);
            var recSize = new Vec(sprite.TextureRect.Width, sprite.TextureRect.Height);
            var aspectRatio = sprite.TextureRect.Height / (float)sprite.TextureRect.Width;
            var spriteSize = ContentRenderSize.ToVec() * Scale;
            if (LockAspectRatio.V)
            {
                var newY = spriteSize.X * aspectRatio;
                spriteSize = new Vec(spriteSize.X, newY);
            }

            var sizeRatio = spriteSize / ContentRenderSize;
            while (sizeRatio.X > Scale.V.X || sizeRatio.Y > Scale.V.Y)
            {
                spriteSize *= 0.9f;
                sizeRatio = spriteSize / ContentRenderSize;
            }

            sprite.Scale = spriteSize / recSize;
            sprite.Position = ContentRenderPos;
            if (HorizontalAlignment.V == Core.HorizontalAlignment.Center)
            {
                sprite.Position = new((ContentRenderPos + (ContentRenderSize / 2 - spriteSize / 2)).X, sprite.Position.Y);
            }
            else if (HorizontalAlignment.V == Core.HorizontalAlignment.Right)
            {
                sprite.Position = new((ContentRenderPos + (ContentRenderSize - spriteSize)).X, sprite.Position.Y);
            }
            if (VerticalAlignment.V == Core.VerticalAlignment.Middle)
            {
                sprite.Position = new(sprite.Position.X, (ContentRenderPos + (ContentRenderSize / 2 - spriteSize / 2)).Y);
            }
            else if (VerticalAlignment.V == Core.VerticalAlignment.Bottom)
            {
                sprite.Position = new(sprite.Position.X, (ContentRenderPos + (ContentRenderSize - spriteSize)).Y);
            }
            target.Draw(sprite, states);
        }
    }
}
