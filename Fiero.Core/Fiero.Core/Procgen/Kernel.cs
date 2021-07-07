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

        public static readonly Mat3 CardinalLaplacian = new(
             0,  0.25f,  0,
             0.25f, -1, 0.25f,
             0, 0.25f,  0
        );


        public static readonly Mat3 Laplacian = new(
            .05f, .2f, .05f,
            .2f, -1f, .2f,
            .05f, .2f, .05f
        );

        public static readonly Mat3 Sharpen = new(
            -.05f, -.2f, -.05f,
            -.2f, 1f, -.2f,
            -.05f, -.2f, -.05f
        );
    }
}
