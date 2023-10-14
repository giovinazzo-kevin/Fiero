namespace Fiero.Business
{
    public partial class Animation
    {
        public readonly AnimationFrame[] Frames;
        public readonly TimeSpan Duration;
        public int RepeatCount { get; set; } = 0;

        public event Action<Animation, int, AnimationFrame> FramePlaying;
        public void OnFramePlaying(int frame) => FramePlaying?.Invoke(this, frame, Frames[frame]);

        public Animation OnFirstFrame(Action a)
        {
            FramePlaying += On;
            return this;
            void On(Animation _, int i, AnimationFrame f)
            {
                if (i == 0)
                {
                    a();
                    FramePlaying -= On;
                }
            }
        }

        public Animation OnLastFrame(Action a)
        {
            FramePlaying += On;
            return this;
            void On(Animation _, int i, AnimationFrame f)
            {
                if (i == Frames.Length - 1)
                {
                    a();
                    FramePlaying -= On;
                }
            }
        }
        public Animation(AnimationFrame[] frames, int repeat = 0)
        {
            Frames = frames;
            Duration = frames.Length == 0 ? TimeSpan.Zero : frames.Select(f => f.Duration).Aggregate((a, b) => a + b);
            RepeatCount = repeat;
        }
    }
}
