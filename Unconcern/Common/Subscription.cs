using System;
using System.Collections.Generic;
using System.Linq;

namespace Unconcern.Common
{

    public class Subscription : IDisposable
    {
        private readonly IEnumerable<Action> _unsub;
        private readonly List<Subscription> _children = new();
        public bool Disposed { get; private set; }
        public bool ThrowIfAlreadyDisposed { get; set; }



        public Subscription(bool throwOnDoubleDispose = true)
        {
            _unsub = Enumerable.Empty<Action>();
            ThrowIfAlreadyDisposed = throwOnDoubleDispose;
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

        public void Add(params Subscription[] other)
        {
            _children.AddRange(other);
        }

        public void Dispose()
        {
            if (Disposed)
            {
                if (ThrowIfAlreadyDisposed)
                    throw new ObjectDisposedException(null);
                else
                    return;
            }
            foreach (var unsub in _unsub)
            {
                unsub();
            }
            for (int i = _children.Count - 1; i >= 0; i--)
            {
                _children[i].Dispose();
                _children.RemoveAt(i);
            }
            Disposed = true;
        }
    }
}
