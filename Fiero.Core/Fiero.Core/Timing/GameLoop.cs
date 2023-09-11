using System.Diagnostics;

namespace Fiero.Core
{

    public class GameLoop
    {
        public TimeSpan TimeStep { get; set; }
        public event Action<TimeSpan, TimeSpan> Tick;
        public event Action<TimeSpan, TimeSpan> Update;
        public event Action<TimeSpan, TimeSpan> Render;
        public TimeSpan T { get; private set; }

        public GameLoop()
        {
            TimeStep = TimeSpan.FromSeconds(1 / 100f);
        }

        public virtual TimeSpan WaitAndDraw(TimeSpan time, Action<TimeSpan, TimeSpan> onUpdate = null, Action<TimeSpan, TimeSpan> onRender = null)
        {
            var innerLoop = new GameLoop() { TimeStep = TimeStep };
            innerLoop.Render += (t, ts) => Render?.Invoke(T, TimeStep);
            if (onUpdate != null) innerLoop.Update += onUpdate;
            if (onRender != null) innerLoop.Render += onRender;
            innerLoop.Run(time);
            return innerLoop.T;
        }

        public virtual void LoopAndDraw(Func<bool> @break, Action<TimeSpan, TimeSpan> onUpdate = null, Action<TimeSpan, TimeSpan> onRender = null)
        {
            var innerLoop = new GameLoop() { TimeStep = TimeStep };
            innerLoop.Render += (t, ts) => Render?.Invoke(T, TimeStep);
            if (onUpdate != null) innerLoop.Update += onUpdate;
            if (onRender != null) innerLoop.Render += onRender;
            innerLoop.Run(@break: @break);
        }

        public TimeSpan Run(TimeSpan duration = default, CancellationToken ct = default, Func<bool> @break = null)
        {
            var stopwatch = Stopwatch.StartNew();
            var accumulator = TimeSpan.Zero;
            var currentTime = stopwatch.Elapsed;
            @break ??= () => false;

            while ((duration == default || stopwatch.Elapsed < duration) && !ct.IsCancellationRequested && !@break())
            {
                var newTime = stopwatch.Elapsed;
                var frameTime = newTime - currentTime;
                if (frameTime > TimeSpan.FromMilliseconds(250))
                {
                    frameTime = TimeSpan.FromMilliseconds(250);
                }
                currentTime = newTime;

                Tick?.Invoke(T, frameTime);
                accumulator += frameTime;

                while (accumulator >= TimeStep)
                {
                    Update?.Invoke(T, TimeStep);
                    T += TimeStep;
                    accumulator -= TimeStep;
                }

                Render?.Invoke(T, TimeStep);

                // Calculate the remaining time to wait until the next frame
                var remainingFrameTime = TimeStep - accumulator;
                if (remainingFrameTime > TimeSpan.Zero)
                {
                    // Use SpinWait to avoid blocking the thread
                    var spinWait = new SpinWait();
                    var targetTime = newTime + remainingFrameTime;
                    while (stopwatch.Elapsed < targetTime && !ct.IsCancellationRequested)
                    {
                        spinWait.SpinOnce();
                    }
                }
            }

            return T;
        }
    }
}
