namespace Fiero.Core
{
    public class GameDatumChangedEventArgs<T>
    {
        public readonly GameDatum<T> Datum;
        public readonly T OldValue;
        public readonly T NewValue;

        public GameDatumChangedEventArgs(GameDatum<T> datum, T oldValue, T newValue)
        {
            Datum = datum;
            OldValue = oldValue;
            NewValue = newValue;
        }

    }
}
