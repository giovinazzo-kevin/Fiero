using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unconcern.Common;

namespace Unconcern.Delegation
{
    public interface IDelegateExpression
    {
        EventBus Bus { get; }
        IEnumerable<Func<EventBus.Message, Task<bool>>> Triggers { get; }
        IEnumerable<Func<EventBus.Message, Task<EventBus.Message>>> Replies { get; }
        IEnumerable<Func<EventBus.Message, Task>> Handlers { get; }
        IEnumerable<IDelegateExpression> Siblings {  get; }
    }
}