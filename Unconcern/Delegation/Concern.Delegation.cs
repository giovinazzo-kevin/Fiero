using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unconcern.Common;
using Unconcern.Deferral;
using Unconcern.Delegation;

namespace Unconcern
{
    public static partial class Concern
    {
        public static IDelegateExpressionBuilder Delegate(EventBus bus = null)
        {
            bus ??= EventBus.Default;
            return new DelegateExpressionBuilder(
                new DelegateExpression(
                    bus,
                    Enumerable.Empty<Func<EventBus.Message, Task<bool>>>(),
                    Enumerable.Empty<Func<EventBus.Message, Task<EventBus.Message>>>(),
                    Enumerable.Empty<Func<EventBus.Message, Task>>(),
                    Enumerable.Empty<IDelegateExpression>()));
        }

        public static partial class Delegation
        {
            public static Task<Subscription> Listen(IDelegateExpression expr, string hub = null) => expr.Listen(hub);
            public static Task Fire(IDelegateExpression expr, string hub) => expr.Fire(hub);
        }
    }
}
