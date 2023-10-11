using System;
using Unconcern.Common;

namespace Unconcern.Delegation
{
    public interface IDelegateExpressionBuilder
    {
        IDelegateExpressionBuilder And(IDelegateExpressionBuilder other);
        IDelegateExpressionBuilder When<T>(Func<EventBus.Message<T>, bool> cond);
        IDelegateExpressionBuilder Send<T, U>(Func<EventBus.Message<T>, EventBus.Message<U>> transform);
        IDelegateExpressionBuilder Reply<T, U>(Func<EventBus.Message<T>, EventBus.Message<U>> transform)
            => Send<T, U>(msg => transform(msg));
        IDelegateExpressionBuilder Do<T>(Action<EventBus.Message<T>> handle, EventBus.MessageHandlerTiming timing);
        IDelegateExpression Build();
    }
}