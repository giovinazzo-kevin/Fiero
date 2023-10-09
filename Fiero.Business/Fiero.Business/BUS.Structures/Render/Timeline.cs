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
        public readonly FloorId Floor;
        public readonly Coord WorldPosition;
        public readonly List<Frame> Frames = new();

        public Timeline(Animation anim, FloorId floor, Coord pos, TimeSpan startAt)
        {
            Animation = anim;
            WorldPosition = pos;
            Floor = floor;
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
