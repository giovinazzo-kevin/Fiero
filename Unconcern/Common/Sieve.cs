using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Unconcern.Common
{
    public class Sieve<T> : IDisposable
    {
        private readonly Subscription _unsub;
        private readonly Func<EventBus.Message<T>, bool> _filter;

        public readonly EventBus Bus;
        public readonly ConcurrentQueue<EventBus.Message<T>> Messages;

        public event Action<Sieve<T>, EventBus.Message<T>> MessageSieved;

        public Sieve(EventBus bus, Func<EventBus.Message<T>, bool> filter)
        {
            Bus = bus;
            _filter = filter;
            Messages = new ConcurrentQueue<EventBus.Message<T>>();
            _unsub = Bus.Register(msg => {
                if(msg.Type.IsAssignableTo(typeof(T))) {
                    var tMsg = new EventBus.Message<T>(msg.Timestamp, (T)msg.Content, msg.Sender, msg.Recipients);
                    if(_filter(tMsg)) {
                        MessageSieved?.Invoke(this, tMsg);
                        Messages.Enqueue(tMsg);
                    }
                }
            });
        }

        public void Dispose()
        {
            _unsub.Dispose();
        }
    }
}
