using Fiero.Core;
using System;
using System.Collections.Generic;

namespace Fiero.Business
{
    public readonly struct Timeline
    {
        public readonly record struct Frame(AnimationFrame AnimFrame, TimeSpan Time)
        {
            public TimeSpan Start => Time;
            public TimeSpan End => Time + AnimFrame.Duration;
        };

        public readonly Animation Animation;
        public readonly Coord ScreenPosition;
        public readonly List<Frame> Frames = new();

        public Timeline(Animation anim, Coord pos, TimeSpan startAt)
        {
            Animation = anim;
            ScreenPosition = pos;
            Frames.AddRange(Get(anim, startAt));
        }

        static IEnumerable<Frame> Get(Animation anim, TimeSpan time)
        {
            for (int i = 0; i < anim.Frames.Length; ++i)
            {
                yield return new(anim.Frames[i], time);
                time += anim.Frames[i].Duration;
            }
        }

    }
}
