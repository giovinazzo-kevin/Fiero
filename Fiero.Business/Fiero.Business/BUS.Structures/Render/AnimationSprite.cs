using Fiero.Core;
using SFML.Graphics;

namespace Fiero.Business
{
    public readonly struct AnimationSprite
    {
        public readonly TextureName Texture;
        public readonly string Sprite;
        public readonly ColorName Color;
        public readonly Vec Offset;
        public readonly Vec Scale;
        public AnimationSprite(TextureName texture, string sprite, ColorName tint, Vec ofs, Vec scale)
        {
            Sprite = sprite;
            Texture = texture;
            Color = tint;
            Offset = ofs;
            Scale = scale;
        }
    }
}
