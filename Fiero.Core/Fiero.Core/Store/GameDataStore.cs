using Unconcern.Common;

namespace Fiero.Core
{

    public class GameDataStore(EventBus bus)
    {
        public const string EventHubName = nameof(GameDataStore);
        public readonly record struct DatumChangedEvent(GameDataStore Sender, GameDatum Datum, object OldValue, object NewValue);

        protected readonly Dictionary<string, object> Data = new();
        protected readonly Dictionary<string, GameDatum> Registry = new();

        public readonly EventBus EventBus = bus;

        /// <summary>
        /// Registers a datum, making it known to the store and enabling script compilation to target specific datum changes.
        /// </summary>
        public void Register(GameDatum datum)
        {
            Registry.Add(datum.Name, datum);
        }

        /// <summary>
        /// Registers all GameDatum instances defined as static fields in the passed type and its nested types.
        /// </summary>
        public void RegisterByReflection(Type lookForFieldsInType)
        {
            foreach (var field in lookForFieldsInType.GetFields())
            {
                if (field.IsStatic && field.FieldType.IsAssignableTo(typeof(GameDatum)))
                    Register((GameDatum)field.GetValue(null));
            }

            foreach (var nestedType in lookForFieldsInType.GetNestedTypes())
                RegisterByReflection(nestedType);
        }

        public IEnumerable<GameDatum> GetRegisteredDatumTypes() => Registry.Values;
        public GameDatum GetRegisteredDatumType(string name) => Registry[name];

        public T GetOrDefault<T>(GameDatum<T> datum, T defaultValue = default)
            => TryGetValue(datum, out var val) ? val : defaultValue;
        public T Get<T>(GameDatum<T> datum)
            => TryGetValue(datum, out var val) ? val : throw new ArgumentException(datum.Name);

        public bool TryGetValue<T>(GameDatum<T> datum, out T value)
        {
            if (Data.TryGetValue(datum.Name, out var obj))
            {
                value = (T)obj;
                return true;
            }
            value = default;
            return false;
        }

        public bool TrySetValue<T>(GameDatum<T> datum, T compare, T newValue)
        {
            if (Data.TryGetValue(datum.Name, out var old) && !Equals(old, compare))
            {
                return false;
            }
            Data[datum.Name] = newValue;
            EventBus.Send(new DatumChangedEvent(this, datum, old, newValue), EventHubName);
            datum.OnValueChanged(compare, newValue);
            return true;
        }

        public void SetValue<T>(GameDatum<T> datum, T newValue) => TrySetValue(datum, GetOrDefault(datum), newValue);
        public void UpdateValue<T>(GameDatum<T> datum, Func<T, T> update)
        {
            var old = GetOrDefault(datum);
            TrySetValue(datum, old, update(old));
        }
    }
}
