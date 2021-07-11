using System;
using System.Threading.Tasks;
using Unconcern.Common;

namespace Unconcern.Delegation
{
    public interface IDelegateExpressionBuilder
    {
        IDelegateExpressionBuilder And(IDelegateExpressionBuilder other);
        IDelegateExpressionBuilder When<T>(Func<EventBus.Message<T>, Task<bool>> cond);
        IDelegateExpressionBuilder When<T>(Func<EventBus.Message<T>, bool> cond)
            => When<T>(msg => Task.FromResult(cond(msg)));
        IDelegateExpressionBuilder Send<T, U>(Func<EventBus.Message<T>, Task<EventBus.Message<U>>> transform);
        IDelegateExpressionBuilder Reply<T, U>(Func<EventBus.Message<T>, EventBus.Message<U>> transform)
            => Send<T, U>(msg => Task.FromResult(transform(msg)));
        IDelegateExpressionBuilder Send<U>(Func<EventBus.Message<object>, EventBus.Message<U>> transform)
            => Send<object, U>(msg => Task.FromResult(transform(msg)));
        IDelegateExpressionBuilder Send<U>(Func<EventBus.Message<object>, Task<EventBus.Message<U>>> transform)
            => Send<object, U>(msg => transform(msg));
        IDelegateExpressionBuilder Do<T>(Func<EventBus.Message<T>, Task> handle);
        IDelegateExpressionBuilder Do<T>(Action<EventBus.Message<T>> handle)
            => Do<T>(_ => { handle(_); return Task.CompletedTask; });
        // IDelegateExpressionBuilder Then(IDelegateExpressionBuilder other);

        IDelegateExpression Build();
    }
}