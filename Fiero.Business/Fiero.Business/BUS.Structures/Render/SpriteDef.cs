using SFML.Graphics;

namespace Fiero.Business
{
    public readonly record struct SpriteDef(TextureName Texture, string Sprite, ColorName Tint, Vec Offset, Vec Scale, float Alpha, float Z = 0, IntRect Crop = default)
    {
    }
}
