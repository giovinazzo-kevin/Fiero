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

        public event Action<Animation, int, AnimationFrame> FramePlaying;
        public void OnFramePlaying(int frame) => FramePlaying?.Invoke(this, frame, Frames[frame]);

        public Animation OnFirstFrame(Action a)
        {
            FramePlaying += (_, i, f) => {
                if (i == 0) a();
            };
            return this;
        }

        public Animation OnLastFrame(Action a)
        {
            FramePlaying += (_, i, f) => {
                if (i == Frames.Length - 1) a();
            };
            return this;
        }

        public Animation(params AnimationFrame[] frames)
        {
            Frames = frames;
            Duration = frames.Select(f => f.Duration).Aggregate((a, b) => a + b);
        }
    }
}
