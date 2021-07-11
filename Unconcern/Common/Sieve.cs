using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Unconcern.Common
{
    public class Sieve<T> : IAsyncDisposable
    {
        private readonly Subscription _unsub;
        private readonly Func<EventBus.Message<T>, bool> _filter;
        public readonly ConcurrentQueue<EventBus.Message<T>> Responses;
        public readonly EventBus Bus;
        public Sieve(EventBus bus, Func<EventBus.Message<T>, bool> filter)
        {
            Bus = bus;
            _filter = filter;
            Responses = new ConcurrentQueue<EventBus.Message<T>>();
            _unsub = Bus.Register(msg => {
                if(msg.Type.IsAssignableTo(typeof(T))) {
                    var tMsg = new EventBus.Message<T>(msg.Timestamp, (T)msg.Content, msg.Sender, msg.Recipients);
                    if(_filter(tMsg)) {
                        Responses.Enqueue(tMsg);
                    }
                }
            });
        }

        public ValueTask DisposeAsync()
        {
            return _unsub.DisposeAsync();
        }
    }
}
