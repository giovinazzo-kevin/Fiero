using System;

namespace Fiero.Business
{
    public readonly struct AnimationFrame
    {
        public readonly SpriteDef[] Sprites;
        public readonly TimeSpan Duration;

        public AnimationFrame(TimeSpan dur, params SpriteDef[] sprites)
        {
            Sprites = sprites;
            Duration = dur;
        }
    }
}
