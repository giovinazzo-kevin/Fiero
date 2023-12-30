namespace Fiero.Core
{
    public abstract class GameDatum
    {
        public readonly Type T;
        public readonly string Module;
        public readonly string Name;
        public readonly bool IsStatic;
        public GameDatum(Type t, string mod, string name, bool isStatic) => (T, Module, Name, IsStatic) = (t, mod, name, isStatic);
        public abstract void OnValueChanged(object oldValue, object newValue);
        public override int GetHashCode() => HashCode.Combine(T, Module, Name);
    }

    public class GameDatum<T> : GameDatum
    {
        public event Action<GameDatumChangedEventArgs<T>> ValueChanged;

        public override void OnValueChanged(object oldValue, object newValue)
            => ValueChanged?.Invoke(new(this, (T)oldValue, (T)newValue));

        public GameDatum(string module, string name, bool isStatic = true) : base(typeof(T), module, name, isStatic) { }
    }
}
