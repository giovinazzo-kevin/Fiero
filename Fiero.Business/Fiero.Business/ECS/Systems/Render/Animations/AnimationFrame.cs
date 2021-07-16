using System;

namespace Fiero.Business
{

    public readonly struct AnimationFrame
    {
        public readonly AnimationSprite[] Sprites;
        public readonly TimeSpan Duration;

        public AnimationFrame(TimeSpan dur, params AnimationSprite[] sprites)
        {
            Sprites = sprites;
            Duration = dur;
        }
    }
}
