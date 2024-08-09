using Ergo.Lang;
using SFML.Graphics;

namespace Fiero.Core
{
    [Term(Marshalling = TermMarshalling.Named)]
    public readonly record struct SpriteDef(
        string Texture,
        string Name,
        string Tint = default,
        Vec Offset = default,
        Vec Scale = default,
        float Alpha = 1,
        float Z = 0,
        IntRect Crop = default,
        bool Relative = true,
        int Rotation = 0,
        int? RngSeed = null)
    {
    }
}
