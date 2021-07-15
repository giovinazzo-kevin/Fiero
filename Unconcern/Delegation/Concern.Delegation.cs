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
                    Enumerable.Empty<Func<EventBus.Message, bool>>(),
                    Enumerable.Empty<Func<EventBus.Message, EventBus.Message>>(),
                    Enumerable.Empty<Action<EventBus.Message>>(),
                    Enumerable.Empty<IDelegateExpression>()));
        }

        public static partial class Delegation
        {
            public static Subscription Listen(IDelegateExpression expr, string hub = null) => expr.Listen(hub);
            public static void Fire(IDelegateExpression expr, string hub) => expr.Fire(hub);
        }
    }
}
