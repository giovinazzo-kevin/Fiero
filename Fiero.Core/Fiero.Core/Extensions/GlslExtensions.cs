using SFML.Graphics;
using SFML.Graphics.Glsl;

namespace Fiero.Core.Extensions
{
    public static class GlslExtensions
    {
        public static Vec4 ToVec4(this Color c) => new(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
        public static Color ToColor(this Vec4 c) => new((byte)(c.X * 255), (byte)(c.Y * 255), (byte)(c.Z * 255), (byte)(c.W * 255));
    }
}
