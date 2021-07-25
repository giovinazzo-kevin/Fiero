namespace Fiero.Business
{
    public partial class RenderSystem
    {
        public readonly struct AnimationFramePlayedEvent
        {
            public readonly Animation Animation;
            public readonly int FrameIndex;
            public AnimationFramePlayedEvent(Animation a, int f) 
                => (Animation, FrameIndex) = (a, f);
        }
    }
}
