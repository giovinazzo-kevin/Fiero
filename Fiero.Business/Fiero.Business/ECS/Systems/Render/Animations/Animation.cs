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
    }
}
