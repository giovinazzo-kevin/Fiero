using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Unconcern.Common
{

    public class Subscription : IDisposable
    {
        private readonly IEnumerable<Action> _unsub;
        private bool _unsubbed = false;
        public bool ThrowIfAlreadyDisposed { get; }

        public Subscription()
        {
            _unsub = Enumerable.Empty<Action>();
            ThrowIfAlreadyDisposed = false;
        }

        public Subscription(IEnumerable<Action> unsub, bool throwOnDoubleDispose = true)
        {
            _unsub = unsub;
            ThrowIfAlreadyDisposed = throwOnDoubleDispose;
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

        public void Dispose()
        {
            if (_unsubbed) {
                if (ThrowIfAlreadyDisposed)
                    throw new ObjectDisposedException(null);
                else
                    return;
            }
            foreach (var unsub in _unsub) {
                unsub();
            }
            _unsubbed = true;
        }
    }
}
