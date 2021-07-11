using System;
using System.Threading;
using System.Threading.Tasks;
using Unconcern.Deferral;

namespace Unconcern
{
    public static partial class Concern
    {
        public static IDeferExpressionBuilder Defer()
        {
            return new DeferExpressionBuilder(
                new DeferExpression(
                    Array.Empty<Func<CancellationToken, Task>>(),
                    TimeSpan.Zero,
                    TimeSpan.MaxValue,
                    null,
                    null,
                    null,
                    false,
                    false));
        }

        public static partial class Deferral
        {
            public static Task Once(IDeferExpression expr, CancellationToken ct = default) => expr.Now(ct);
            public static Task LoopForever(IDeferExpression expr, CancellationToken ct = default) => expr.LoopForever(ct);
        }
    }
}
