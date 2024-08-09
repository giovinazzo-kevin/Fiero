using System.Diagnostics;

namespace Fiero.Core
{

    [SingletonDependency]
    public class GameLoop
    {
        public TimeSpan TimeStep { get; set; }
        public TimeSpan MaxTimeStep { get; set; }
        public event Action<TimeSpan, TimeSpan> Update;
        public event Action<TimeSpan, TimeSpan> Render;
        public TimeSpan T { get; private set; }

        public double FPS => 1 / _averageFrameTime;
        private double _averageFrameTime;

        public GameLoop()
        {
            TimeStep = TimeSpan.FromSeconds(1 / 100f);
            MaxTimeStep = TimeStep;
        }

        public virtual TimeSpan WaitAndDraw(TimeSpan time, Action<TimeSpan, TimeSpan> onUpdate = null, Action<TimeSpan, TimeSpan> onRender = null)
        {
            if (time <= TimeSpan.Zero)
                return T;
            var innerLoop = new GameLoop() { TimeStep = TimeStep };
            innerLoop.Render += (t, ts) => Render?.Invoke(T, TimeStep);
            innerLoop.Update += (t, ts) =>
            {
                T += ts;
                onUpdate?.Invoke(T, TimeStep);
            };
            innerLoop.Render += (t, dt) =>
            {
                onRender?.Invoke(T, dt);
                UpdateAverageFrameTime(dt);
            };
            innerLoop.Run(time);
            return innerLoop.T;
        }

        public virtual TimeSpan LoopAndDraw(Func<bool> @break, Action<TimeSpan, TimeSpan> onUpdate = null, Action<TimeSpan, TimeSpan> onRender = null)
        {
            var innerLoop = new GameLoop() { TimeStep = TimeStep };
            innerLoop.Render += (t, ts) => Render?.Invoke(T, TimeStep);
            innerLoop.Update += (t, ts) =>
            {
                T += ts;
                onUpdate?.Invoke(T, TimeStep);
            };
            innerLoop.Render += (t, dt) =>
            {
                onRender?.Invoke(T, dt);
                UpdateAverageFrameTime(dt);
            };
            innerLoop.Run(@break: @break);
            return innerLoop.T;
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
                if (frameTime > MaxTimeStep)
                {
                    frameTime = MaxTimeStep;
                }
                currentTime = newTime;
                accumulator += frameTime;

                while (accumulator >= TimeStep)
                {
                    Update?.Invoke(T, TimeStep);
                    T += TimeStep;
                    accumulator -= TimeStep;
                }

                Render?.Invoke(T, frameTime);
                UpdateAverageFrameTime(frameTime);

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
        private void UpdateAverageFrameTime(TimeSpan frameTime)
        {
            const double smoothingFactor = 0.95;
            _averageFrameTime = smoothingFactor * _averageFrameTime + (1 - smoothingFactor) * frameTime.TotalSeconds;
        }
    }
}
