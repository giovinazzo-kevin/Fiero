using System;
using System.Threading.Tasks;
using Unconcern.Common;

namespace Unconcern.Delegation
{
    public class DelegateExpressionBuilder : IDelegateExpressionBuilder
    {
        protected readonly IDelegateExpression Expression;

        public DelegateExpressionBuilder(IDelegateExpression expr)
        {
            Expression = expr;
        }

        public IDelegateExpressionBuilder And(IDelegateExpressionBuilder other)
        {
            return new DelegateExpressionBuilder(Expression.WithSibling(other.Build()));
        }


        public IDelegateExpressionBuilder Do<T>(Action<EventBus.Message<T>> handle)
        {
            return new DelegateExpressionBuilder(Expression.WithHandler(msg => {
                if (!msg.Type.IsAssignableTo(typeof(T)))
                    throw new InvalidCastException("wrong_handler");
                var content = (T)msg.Content;
                handle(new EventBus.Message<T>(msg.Timestamp, content, msg.Sender, msg.Recipients));
            }));
        }

        public IDelegateExpressionBuilder Send<T, U>(Func<EventBus.Message<T>, EventBus.Message<U>> transform)
        {
            return new DelegateExpressionBuilder(Expression.WithReply(msg => {
                if (!msg.Type.IsAssignableTo(typeof(T)))
                    throw new InvalidCastException("wrong_reply");
                var content = (T)msg.Content;
                var transformed = transform(new EventBus.Message<T>(msg.Timestamp, content, msg.Sender, msg.Recipients));
                return new EventBus.Message(DateTime.Now, typeof(U), transformed.Content, transformed.Sender, transformed.Recipients);
            }));
        }

        public IDelegateExpressionBuilder When<T>(Func<EventBus.Message<T>, bool> cond)
        {
            return new DelegateExpressionBuilder(Expression.WithCondition(msg => {
                if (!msg.Type.IsAssignableTo(typeof(T)))
                    return false;
                var content = (T)msg.Content;
                return cond(new EventBus.Message<T>(msg.Timestamp, content, msg.Sender, msg.Recipients));
            }));
        }
        public IDelegateExpression Build()
        {
            return Expression;
        }
    }
}