using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unconcern.Common;

namespace Unconcern.Delegation
{
    public class DelegateExpression : IDelegateExpression
    {
        public EventBus Bus { get; private set; }

        protected List<Func<EventBus.Message, Task<bool>>> TriggersList;
        public IEnumerable<Func<EventBus.Message, Task<bool>>> Triggers => TriggersList;

        protected List<Func<EventBus.Message, Task<EventBus.Message>>> RepliesList;
        public IEnumerable<Func<EventBus.Message, Task<EventBus.Message>>> Replies => RepliesList;

        protected List<Func<EventBus.Message, Task>> HandlersList;
        public IEnumerable<Func<EventBus.Message, Task>> Handlers => HandlersList;

        protected List<IDelegateExpression> SiblingsList;
        public IEnumerable<IDelegateExpression> Siblings => SiblingsList;


        internal DelegateExpression(
            EventBus bus, 
            IEnumerable<Func<EventBus.Message, Task<bool>>> triggers,
            IEnumerable<Func<EventBus.Message, Task<EventBus.Message>>> replies,
            IEnumerable<Func<EventBus.Message, Task>> handlers,
            IEnumerable<IDelegateExpression> siblings)
        {
            Bus = bus;
            TriggersList = new List<Func<EventBus.Message, Task<bool>>>(triggers);
            RepliesList = new List<Func<EventBus.Message, Task<EventBus.Message>>>(replies);
            HandlersList = new List<Func<EventBus.Message, Task>>(handlers);
            SiblingsList = new List<IDelegateExpression>(siblings);
        }
    }
}