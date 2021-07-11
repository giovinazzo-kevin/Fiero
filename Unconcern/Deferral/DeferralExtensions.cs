using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Unconcern.Deferral
{
    public static class DeferralExtensions
    {
        public static Task LoopForever(this IDeferExpression expr, CancellationToken ct = default)
        {
            return Task.Run(async () => {
                while(true) {
                    await expr.Now(ct);
                }
            }, ct);
        }

        public static Task Now(this IDeferExpression expression, CancellationToken externalToken = default)
        {
            return expression.Now(externalToken, null, false);
        }

        internal static async Task Now(this IDeferExpression expression, CancellationToken externalToken, IEnumerable<IDeferExpression> fallbacks = null, bool tightLoop = false)
        {
            fallbacks ??= Enumerable.Empty<IDeferExpression>();
            var tasks = expression.Siblings.Prepend(expression)
                .Select(e => {
                    if(e.PreciseTimer || tightLoop) {
                        return Task.Run(() => Sandbox(e, fallbacks).GetAwaiter().GetResult());
                    }
                    return Sandbox(e, fallbacks);
                });
            await Task.WhenAll(tasks);
            tasks = expression.Siblings.Prepend(expression)
                .SelectMany(s => s.Children.Select(c => c.Now(externalToken, fallbacks.Concat(s.Fallbacks), tightLoop | expression.PreciseTimer)));
            await Task.WhenAll(tasks);
            async Task Sandbox(IDeferExpression expression, IEnumerable<IDeferExpression> fallbacks)
            {
                var watch = new Stopwatch();
                watch.Start();
                var preciseTimer = tightLoop || expression.PreciseTimer;
                using var durationTokenSource = expression.Duration < TimeSpan.MaxValue
                    ? new CancellationTokenSource(expression.Duration)
                    : new CancellationTokenSource();
                using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(externalToken, durationTokenSource.Token);
                var fb = fallbacks.Concat(expression.Fallbacks)
                    .ToArray();
                if (watch.Elapsed < expression.Delay) {
                    if(!preciseTimer) {
                        await AsyncDelay(expression.Delay - watch.Elapsed);
                    }
                    else {
                        SyncDelay(expression.Delay - watch.Elapsed, watch);
                    }
                }
                try {
                    watch.Restart();
                    if (!preciseTimer) {
                        if(expression.WaitDuration) {
                            await Task.WhenAny(
                                Task.Delay(expression.Duration, tokenSource.Token),
                                Task.WhenAll(expression.Tasks.Select(t => t(tokenSource.Token))));
                        }
                        else {
                            await Task.WhenAll(expression.Tasks.Select(t => t(tokenSource.Token)));
                        }
                    }
                    else {
                        foreach (var t in expression.Tasks) {
                            t(tokenSource.Token)
                                .GetAwaiter()
                                .GetResult();
                        }
                    }
                    if (expression.Duration < TimeSpan.MaxValue
                        && watch.Elapsed < expression.Duration) {
                        if (!preciseTimer) {
                            if(expression.WaitDuration) {
                                await AsyncDelay(expression.Duration - watch.Elapsed);
                            }
                        }
                        else {
                            if (expression.WaitDuration) {
                                SyncDelay(expression.Duration - watch.Elapsed, watch);
                            }
                        }
                    }
                }
                catch(Exception e) when (fb.Length > 0 && (e is TaskCanceledException || e is OperationCanceledException)) {
                    for (int i = 0; i < fb.Length - 1; i++) {
                        try {
                            await fb[i].Now(externalToken, null, preciseTimer);
                            return;
                        }
                        catch(TaskCanceledException) { }
                        catch(OperationCanceledException) { }
                    }
                    await fb[^1].Now(externalToken, null, preciseTimer);
                    return;
                }
            }

            Task AsyncDelay(TimeSpan delay)
            {
                if (delay > TimeSpan.Zero)
                    return Task.Delay(delay);
                return Task.CompletedTask;
            }

            void SyncDelay(TimeSpan delay, Stopwatch sw)
            {
                var spinWait = new SpinWait();
                sw.Restart();
                while(sw.Elapsed < delay) {
                    spinWait.SpinOnce(-1);
                }
                sw.Stop();
            }
        }
        internal static IDeferExpression WithSibling(this IDeferExpression self, IDeferExpression c)
        {
            return new DeferExpression(self.Tasks, self.Delay, self.Duration, self.Siblings.Append(c), self.Children, self.Fallbacks, self.WaitDuration, self.PreciseTimer);
        }
        internal static IDeferExpression WithSiblings(this IDeferExpression self, IEnumerable<IDeferExpression> siblings)
        {
            return new DeferExpression(self.Tasks, self.Delay, self.Duration, siblings, self.Children, self.Fallbacks, self.WaitDuration, self.PreciseTimer);
        }
        internal static IDeferExpression WithChild(this IDeferExpression self, IDeferExpression c)
        {
            return new DeferExpression(self.Tasks, self.Delay, self.Duration, self.Siblings, self.Children.Append(c), self.Fallbacks, self.WaitDuration, self.PreciseTimer);
        }
        internal static IDeferExpression WithChildren(this IDeferExpression self, IEnumerable<IDeferExpression> children)
        {
            return new DeferExpression(self.Tasks, self.Delay, self.Duration, self.Siblings, children, self.Fallbacks, self.WaitDuration, self.PreciseTimer);
        }
        internal static IDeferExpression WithFallback(this IDeferExpression self, IDeferExpression c)
        {
            return new DeferExpression(self.Tasks, self.Delay, self.Duration, self.Siblings, self.Children, self.Fallbacks.Append(c), self.WaitDuration, self.PreciseTimer);
        }
        internal static IDeferExpression WithStart(this IDeferExpression self, TimeSpan newStart)
        {
            if(!self.Siblings.Any()) {
                return new DeferExpression(self.Tasks, newStart, self.Duration, self.Siblings, self.Children, self.Fallbacks, self.WaitDuration, self.PreciseTimer);
            }
            return new DeferExpression(Array.Empty<Func<CancellationToken, Task>>(), newStart, TimeSpan.MaxValue, null, new[] { self }, null, false, self.PreciseTimer);
        }
        internal static IDeferExpression WithDuration(this IDeferExpression self, TimeSpan newDuration, bool waitFullDuration = false)
        {
            return new DeferExpression(self.Tasks, self.Delay, newDuration, self.Siblings, self.Children, self.Fallbacks, waitFullDuration, self.PreciseTimer);

        }
        internal static IDeferExpression WithPreciseTiming(this IDeferExpression self, bool preciseTiming)
        {
            return new DeferExpression(self.Tasks, self.Delay, self.Duration, self.Siblings, self.Children, self.Fallbacks, self.WaitDuration, preciseTiming);
        }
        internal static IDeferExpression WithTask(this IDeferExpression self, Func<CancellationToken, Task> task)
        {
            return new DeferExpression(self.Tasks.Append(task), self.Delay, self.Duration, self.Siblings, self.Children, self.Fallbacks, self.WaitDuration, self.PreciseTimer);
        }
    }
}
