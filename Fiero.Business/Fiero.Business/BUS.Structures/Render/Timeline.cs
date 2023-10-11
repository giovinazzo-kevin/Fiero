using Ergo.Lang;

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
        public readonly Either<Location, PhysicalEntity> At;
        public readonly List<Frame> Frames = new();

        public Coord WorldPos => At.Reduce(l => l.Position, e => !e.IsInvalid() ? e.Physics.Position : Coord.Zero);
        public bool Visible => At.Reduce(l => true, e => !e.IsInvalid() && !e.Render.Hidden);
        public Vec Offset => At.Reduce(l => Vec.Zero, e => e.TryCast<Actor>(out var a) && a.Faction.Name != FactionName.None
            ? new Vec(0f, -0.33f) : Vec.Zero);

        public Timeline(Animation anim, Either<Location, PhysicalEntity> pos, TimeSpan startAt)
        {
            Animation = anim;
            At = pos;
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
