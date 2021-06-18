using SFML.Graphics.Glsl;

namespace Fiero.Core
{
    public static class Kernel
    {
        private const float ONE_NINTH = 1 / 9f;
        public static readonly Mat3 GaussianBlur = new(
            ONE_NINTH, ONE_NINTH, ONE_NINTH,
            ONE_NINTH, ONE_NINTH, ONE_NINTH,
            ONE_NINTH, ONE_NINTH, ONE_NINTH
        );

        public static readonly Mat3 Laplacian = new(
             0,  1,  0,
             1, -4,  1,
             0,  1,  0
        );

        public static readonly Mat3 Sharpen = new(
             0, -1,  0,
            -1,  5, -1,
             0, -1,  0
        );
    }
}
