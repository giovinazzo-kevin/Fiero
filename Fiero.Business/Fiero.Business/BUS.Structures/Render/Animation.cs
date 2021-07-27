using Fiero.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    public partial class Animation
    {
        public readonly AnimationFrame[] Frames;
        public readonly TimeSpan Duration;

        public event Action<Animation, AnimationFrame> FramePlaying;
        public void OnFramePlaying(int frame) => FramePlaying?.Invoke(this, Frames[frame]);

        public Animation(params AnimationFrame[] frames)
        {
            Frames = frames;
            Duration = frames.Select(f => f.Duration).Aggregate((a, b) => a + b);
        }
    }
}
