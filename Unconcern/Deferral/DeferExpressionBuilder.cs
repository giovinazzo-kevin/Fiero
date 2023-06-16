using System;
using System.Threading;
using System.Threading.Tasks;

namespace Unconcern.Deferral
{
    public class DeferExpressionBuilder : IDeferExpressionBuilder
    {
        private readonly IDeferExpression _expression;

        internal DeferExpressionBuilder(IDeferExpression e)
        {
            _expression = e;
        }

        public IDeferExpressionBuilder Do(Action a)
        {
            return new DeferExpressionBuilder(_expression.WithTask(t => { t.ThrowIfCancellationRequested(); a(); t.ThrowIfCancellationRequested(); return Task.CompletedTask; }));
        }

        public IDeferExpressionBuilder Do(Func<CancellationToken, Task> a)
        {
            return new DeferExpressionBuilder(_expression.WithTask(async t => { await a(t); }));
        }

        public IDeferExpressionBuilder And(IDeferExpressionBuilder other)
        {
            return new DeferExpressionBuilder(_expression.WithSibling(other.Build()));
        }

        public IDeferExpressionBuilder Else(IDeferExpressionBuilder other)
        {
            return new DeferExpressionBuilder(_expression.WithFallback(other.Build()));
        }

        public IDeferExpressionBuilder Then(IDeferExpressionBuilder other)
        {
            return new DeferExpressionBuilder(_expression.WithChild(other.Build()));
        }

        public IDeferExpressionBuilder After(TimeSpan delay)
        {
            return new DeferExpressionBuilder(_expression.WithStart(delay));
        }

        public IDeferExpressionBuilder WaitAtMost(TimeSpan maxDuration)
        {

            return new DeferExpressionBuilder(_expression.WithDuration(maxDuration, waitFullDuration: false));
        }
        public IDeferExpressionBuilder WaitExactly(TimeSpan maxDuration)
        {
            return new DeferExpressionBuilder(_expression.WithDuration(maxDuration, waitFullDuration: true));
        }

        public IDeferExpression Build()
        {
            return _expression;
        }

        public IDeferExpressionBuilder UseSynchronousTimer()
        {
            return new DeferExpressionBuilder(_expression.WithPreciseTiming(true));
        }

        public IDeferExpressionBuilder UseAsynchronousTimer()
        {
            return new DeferExpressionBuilder(_expression.WithPreciseTiming(false));
        }
    }
}
