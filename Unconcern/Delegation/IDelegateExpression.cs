using System;
using System.Collections.Generic;
using Unconcern.Common;

namespace Unconcern.Delegation
{
    public interface IDelegateExpression
    {
        EventBus Bus { get; }
        IEnumerable<Func<EventBus.Message, bool>> Triggers { get; }
        IEnumerable<Func<EventBus.Message, EventBus.Message>> Replies { get; }
        IEnumerable<Action<EventBus.Message>> Handlers { get; }
        IEnumerable<IDelegateExpression> Siblings { get; }
    }
}