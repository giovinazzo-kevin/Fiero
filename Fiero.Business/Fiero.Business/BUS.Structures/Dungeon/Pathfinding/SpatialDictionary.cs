using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Fiero.Business
{
    internal class SpatialDictionary<V, K> : IReadOnlyDictionary<Coord, V>
        where V : IPathNode<K>
    {
        private readonly V[,] _values;
        private readonly HashSet<Coord> _keys;
        public readonly Coord Size;

        public IEnumerable<Coord> Keys => _keys;
        public IEnumerable<V> Values => _keys.Select(k => _values[k.X, k.Y]);
        public int Count => Size.X * Size.Y;
        public V this[Coord key]
        {
            get => _values[key.X, key.Y];
            set
            {
                var contains = _keys.Contains(key);
                var isNull = value == null;
                if (contains && isNull)
                {
                    _keys.Remove(key);
                }
                else if (!contains && !isNull)
                {
                    _keys.Add(key);
                }
                _values[key.X, key.Y] = value;
            }
        }

        public SpatialDictionary(Coord size)
        {
            Size = size;
            _values = new V[size.X, size.Y];
            _keys = new HashSet<Coord>();
        }

        public bool KeyInBounds(Coord key) => key.X >= 0 && key.Y >= 0 && key.X < Size.X && key.Y < Size.Y;
        public bool ContainsKey(Coord key) => _keys.Contains(key);
        public bool TryGetValue(Coord key, [MaybeNullWhen(false)] out V value)
        {
            if (ContainsKey(key))
            {
                value = _values[key.X, key.Y];
                return true;
            }
            value = default;
            return false;
        }

        public IEnumerator<KeyValuePair<Coord, V>> GetEnumerator() => _keys
                .Select(p => new KeyValuePair<Coord, V>(p, _values[p.X, p.Y]))
                .GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public SpatialAStar<V, K> GetPathfinder() => new(_values);
    }
}
