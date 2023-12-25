namespace Fiero.Core
{
    public abstract class GameDatum
    {
        public readonly Type T;
        public readonly string Name;

        public GameDatum(Type t, string name) => (T, Name) = (t, name);

        public void Deconstruct(out Type type, out string name)
        {
            type = T;
            name = Name;
        }

        public override int GetHashCode() => HashCode.Combine(T, Name);
    }

    public class GameDatum<T> : GameDatum
    {
        public event Action<GameDatumChangedEventArgs<T>> ValueChanged;

        internal void OnValueChanged(T oldValue, T newValue)
            => ValueChanged?.Invoke(new(this, oldValue, newValue));

        public GameDatum(string name) : base(typeof(T), name) { }
    }
}
