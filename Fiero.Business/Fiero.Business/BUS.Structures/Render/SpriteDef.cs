using SFML.Graphics;

namespace Fiero.Business
{
    public readonly record struct SpriteDef(
        string Texture,
        string Sprite,
        string Tint,
        Vec Offset,
        Vec Scale,
        float Alpha,
        float Z = 0,
        IntRect Crop = default,
        bool Relative = true,
        int Rotation = 0)
    {
    }
}
