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

        public virtual void Wait(TimeSpan time)
        {
            var innerLoop = new GameLoop();
            innerLoop.Run(new CancellationTokenSource(time).Token);
        }

        public virtual void Run(CancellationToken ct = default)
        {
            var time = new Stopwatch();
            time.Start();
            T = 0f;
            var accumulator = 0f;
            var currentTime = (float)time.Elapsed.TotalSeconds;
            while (!ct.IsCancellationRequested) {
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
                    T += TimeStep;
                    accumulator -= TimeStep;
                }
                Render?.Invoke(T, TimeStep);
            }
        }
    }
}
