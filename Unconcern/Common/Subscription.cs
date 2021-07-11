using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Unconcern.Common
{

    public class Subscription : IAsyncDisposable
    {
        private readonly IEnumerable<Func<Task>> _unsub;
        private bool _unsubbed = false;
        public bool ThrowIfAlreadyDisposed { get; }

        public Subscription()
        {
            _unsub = Enumerable.Empty<Func<Task>>();
            ThrowIfAlreadyDisposed = false;
        }

        public Subscription(IEnumerable<Func<Task>> unsub, bool throwOnDoubleDispose = true)
        {
            _unsub = unsub;
            ThrowIfAlreadyDisposed = throwOnDoubleDispose;
        }

        public Subscription(IEnumerable<Action> unsub, bool throwOnRepeatDispose = true)
        {
            _unsub = unsub.Select<Action, Func<Task>>(a => () => { a(); return Task.CompletedTask; });
            ThrowIfAlreadyDisposed = throwOnRepeatDispose;
        }

        public Subscription(IEnumerable<Subscription> subs, bool throwOnDoubleDispose = true)
        {
            _unsub = subs.SelectMany(s => s._unsub);
            ThrowIfAlreadyDisposed = throwOnDoubleDispose;
        }

        public Subscription Add(params Subscription[] other)
        {
            return new Subscription(other.Prepend(this), ThrowIfAlreadyDisposed);
        }

        public static Subscription operator +(Subscription self, Subscription other)
        {
            return self.Add(other);
        }

        public async ValueTask DisposeAsync()
        {
            if (_unsubbed) {
                if (ThrowIfAlreadyDisposed)
                    throw new ObjectDisposedException(null);
                else
                    return;
            }
            foreach (var unsub in _unsub) {
                await unsub();
            }
            _unsubbed = true;
        }
    }
}
