namespace Fiero.Core
{
    public abstract class GameDatum
    {
        public readonly Type T;
        public readonly string Name;

        public GameDatum(Type t, string name) => (T, Name) = (t, name);

        public abstract void OnValueChanged(object oldValue, object newValue);

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

        public override void OnValueChanged(object oldValue, object newValue)
            => ValueChanged?.Invoke(new(this, (T)oldValue, (T)newValue));

        public GameDatum(string name) : base(typeof(T), name) { }
    }
}
