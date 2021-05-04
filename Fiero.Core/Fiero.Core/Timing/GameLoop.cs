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

        public float CurrentTime { get; private set; }

        public GameLoop()
        {
            TimeStep = 1f / 100f;
        }

        public virtual void Run(CancellationToken ct = default)
        {
            var time = new Stopwatch();
            time.Start();
            var (t, accumulator) =
                (0f, 0f);
            CurrentTime = (float)time.Elapsed.TotalSeconds;
            while (!ct.IsCancellationRequested) {
                Tick?.Invoke(t, TimeStep);
                var newTime = (float)time.Elapsed.TotalSeconds;
                var frameTime = newTime - CurrentTime;
                if (frameTime > 0.25f) {
                    frameTime = 0.25f;
                }
                CurrentTime = newTime;
                accumulator += frameTime;
                while (accumulator >= TimeStep) {
                    Update?.Invoke(t, TimeStep);
                    t += TimeStep;
                    accumulator -= TimeStep;
                }
                Render?.Invoke(t, TimeStep);
            }
        }
    }
}
