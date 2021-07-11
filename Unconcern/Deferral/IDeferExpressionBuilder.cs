using System;
using System.Threading;
using System.Threading.Tasks;

namespace Unconcern.Deferral
{
    public interface IDeferExpressionBuilder
    {
        IDeferExpressionBuilder And(IDeferExpressionBuilder other);
        IDeferExpressionBuilder Then(IDeferExpressionBuilder other);
        IDeferExpressionBuilder Else(IDeferExpressionBuilder other);
        IDeferExpressionBuilder After(TimeSpan delay);
        IDeferExpressionBuilder WaitAtMost(TimeSpan maxDuration);
        IDeferExpressionBuilder WaitExactly(TimeSpan maxDuration);
        IDeferExpressionBuilder UseSynchronousTimer();
        IDeferExpressionBuilder UseAsynchronousTimer();
        IDeferExpressionBuilder Do(Func<CancellationToken, Task> a);
        IDeferExpressionBuilder Do(Action a);

        IDeferExpression Build();
    }
}
