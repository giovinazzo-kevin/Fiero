using Fiero.Core;

namespace Fiero.Business
{
    public readonly struct AnimationSprite
    {
        public readonly TextureName Texture;
        public readonly string Sprite;
        public readonly Coord Offset;
        public AnimationSprite(TextureName texture, string sprite, Coord ofs)
        {
            Sprite = sprite;
            Texture = texture;
            Offset = ofs;
        }
    }
}
