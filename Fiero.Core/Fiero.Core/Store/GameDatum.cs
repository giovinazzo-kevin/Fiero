using System;

namespace Fiero.Core
{
    public class GameDatum
    {
        public readonly Type T;
        public readonly string Name;

        public GameDatum(Type t, string name) => (T, Name) = (t, name);
    }

    public class GameDatum<T> : GameDatum
    {
        public event Action<GameDatumChangedEventArgs<T>> ValueChanged;

        internal void OnValueChanged(T oldValue, T newValue) 
            => ValueChanged?.Invoke(new(this, oldValue, newValue));

        public GameDatum(string name) : base(typeof(T), name) { }
    }
}
