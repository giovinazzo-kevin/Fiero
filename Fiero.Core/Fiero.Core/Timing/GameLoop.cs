using System;
using System.Diagnostics;
using System.Threading;

namespace Fiero.Core
{

    public class GameLoop
    {
        public float TimeStep { get; set; }
        public event Action<float, float> Tick;
        public event Action<float, float> Update;
        public event Action<float, float> Render;
        public float T { get; private set; }

        public GameLoop()
        {
            TimeStep = 1f / 500f;
        }

        public virtual float WaitAndDraw(TimeSpan time, Action<float, float> onUpdate = null, Action<float, float> onRender = null)
        {
            var innerLoop = new GameLoop() { TimeStep = TimeStep };
            innerLoop.Render += (t, ts) => Render?.Invoke(T, TimeStep);
            if (onUpdate != null) innerLoop.Update += onUpdate;
            if (onRender != null) innerLoop.Render += onRender;
            innerLoop.Run(time);
            return innerLoop.T;
        }

        public virtual void LoopAndDraw(Func<bool> @break, Action<float, float> onUpdate = null, Action<float, float> onRender = null)
        {
            var innerLoop = new GameLoop() { TimeStep = TimeStep };
            innerLoop.Render += (t, ts) => Render?.Invoke(T, TimeStep);
            if (onUpdate != null) innerLoop.Update += onUpdate;
            if (onRender != null) innerLoop.Render += onRender;
            innerLoop.Run(@break: @break);
        }

        public virtual float Run(TimeSpan duration = default, Func<bool> @break = null, CancellationToken ct = default)
        {
            @break ??= () => false;
            var time = new Stopwatch();
            time.Start();
            T = 0f;
            var accumulator = 0f;
            var currentTime = (float)time.Elapsed.TotalSeconds;
            while ((duration.TotalSeconds == 0 || time.Elapsed < duration) && !ct.IsCancellationRequested) {
                var newTime = (float)time.Elapsed.TotalSeconds;
                var frameTime = newTime - currentTime;
                if (frameTime > 0.25f) {
                    frameTime = 0.25f;
                }
                currentTime = newTime;
                Tick?.Invoke(T, TimeStep);
                accumulator += frameTime;
                while (accumulator >= TimeStep) {
                    Update?.Invoke(T, TimeStep);
                    if (@break()) return T;
                    T += TimeStep;
                    accumulator -= TimeStep;
                }
                Render?.Invoke(T, TimeStep);
            }
            return T;
        }
    }
}
