using Fiero.Core;
using SFML.Graphics;

namespace Fiero.Business
{
    public readonly struct AnimationSprite
    {
        public readonly TextureName Texture;
        public readonly string Sprite;
        public readonly ColorName Tint;
        public readonly Coord Offset;
        public AnimationSprite(TextureName texture, string sprite, ColorName tint, Coord ofs)
        {
            Sprite = sprite;
            Texture = texture;
            Tint = tint;
            Offset = ofs;
        }
    }
}
