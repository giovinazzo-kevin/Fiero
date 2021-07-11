using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Unconcern.Deferral
{
    public interface IDeferExpression
    {
        bool WaitDuration { get; }
        bool PreciseTimer { get; }
        TimeSpan Delay { get; }
        TimeSpan Duration { get; }
        IEnumerable<IDeferExpression> Siblings { get; }
        IEnumerable<IDeferExpression> Children { get; }
        IEnumerable<IDeferExpression> Fallbacks { get; }
        IEnumerable<Func<CancellationToken, Task>> Tasks { get; }
    }
}
