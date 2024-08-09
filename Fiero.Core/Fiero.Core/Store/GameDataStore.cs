using Unconcern;
using Unconcern.Common;
using Unconcern.Delegation;

namespace Fiero.Core
{

    [SingletonDependency]
    public class GameDataStore(EventBus bus)
    {
        public const string EventHubName = nameof(GameDataStore);
        public readonly record struct DatumChangedEvent(GameDataStore Sender, GameDatum Datum, object OldValue, object NewValue);

        protected readonly Dictionary<string, object> Data = new();
        protected readonly Dictionary<string, GameDatum> Registry = new();

        public readonly EventBus EventBus = bus;

        private static string Key(string module, string name) => module + name;
        private static string Key(GameDatum datum) => Key(datum.Module, datum.Name);

        /// <summary>
        /// Registers a datum, making it known to the store and enabling script compilation to target specific datum changes.
        /// </summary>
        public void Register(GameDatum datum)
        {
            Registry.Add(Key(datum), datum);
        }
        public void Register<T>(GameDatum<T> datum, T defaultValue)
        {
            Registry.Add(Key(datum), datum);
            TrySetValueUntyped(datum, default(T), defaultValue);
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
        public GameDatum GetRegisteredDatumType(string module, string name) => Registry[Key(module, name)];
        public bool TryGetRegisteredDatumType(string module, string name, out GameDatum datum) => Registry.TryGetValue(Key(module, name), out datum);

        public T GetOrDefault<T>(GameDatum<T> datum, T defaultValue = default)
            => TryGetValue(datum, out var val) ? val : defaultValue;
        public T Get<T>(GameDatum<T> datum)
            => TryGetValue(datum, out var val) ? val : throw new ArgumentException(datum.Name);

        public object GetOrDefault(GameDatum datum, object defaultValue = default)
            => Data.TryGetValue(Key(datum), out var val) ? val : defaultValue;
        public object Get(GameDatum datum)
            => Data.TryGetValue(Key(datum), out var val) ? val : throw new ArgumentException(datum.Name);

        public bool TryGetValue<T>(GameDatum<T> datum, out T value)
        {
            if (Data.TryGetValue(Key(datum), out var obj))
            {
                value = (T)obj;
                return true;
            }
            value = default;
            return false;
        }

        public bool TrySetValue<T>(GameDatum<T> datum, T compare, T newValue) => TrySetValueUntyped(datum, compare, newValue);
        private bool TrySetValueUntyped(GameDatum datum, object compare, object newValue)
        {
            var key = Key(datum);
            if (Data.TryGetValue(key, out var old) && !Equals(old, compare))
            {
                return false;
            }
            Data[key] = newValue;
            EventBus.Send(new DatumChangedEvent(this, datum, old, newValue), EventHubName);
            datum.OnValueChanged(compare, newValue);
            return true;
        }

        public void SetValue<T>(GameDatum<T> datum, T newValue) => TrySetValue(datum, GetOrDefault(datum), newValue);
        public void SetValue(GameDatum datum, object newValue) => TrySetValueUntyped(datum, GetOrDefault(datum), newValue);
        public void UpdateValue<T>(GameDatum<T> datum, Func<T, T> update)
        {
            var old = GetOrDefault(datum);
            TrySetValue(datum, old, update(old));
        }

        public Subscription SubscribeHandler(string datumModule, string datumName, Action<DatumChangedEvent> handle)
        {
            return Concern.Delegate(EventBus)
                .When<DatumChangedEvent>(x => (datumModule == null && datumName == null)
                    || datumModule.Equals(x.Content.Datum.Module) && datumName.Equals(x.Content.Datum.Name))
                .Do<DatumChangedEvent>(msg => handle(msg.Content))
                .Build()
                .Listen(EventHubName);
        }
    }
}
