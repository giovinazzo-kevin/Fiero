using Fiero.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    public class Animation
    {
        public readonly AnimationFrame[] Frames;
        public readonly TimeSpan Duration;

        public Animation(params AnimationFrame[] frames)
        {
            Frames = frames;
            Duration = frames.Select(f => f.Duration).Aggregate((a, b) => a + b);
        }

        public static readonly Animation Fireball = new(
            Enumerable.Range(3, 5)
                .Select(i => new AnimationFrame(TimeSpan.FromMilliseconds(10), CoordEnumerable.Circle(3)
                    .Select(p => new AnimationSprite(TextureName.Atlas, "Skull", p))
                    .ToArray()))
                .ToArray()
        );
    }
}
