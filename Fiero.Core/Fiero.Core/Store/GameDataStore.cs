using System;
using System.Collections.Generic;

namespace Fiero.Core
{

    public class GameDataStore
    {
        protected readonly Dictionary<string, object> Data;



        public GameDataStore()
        {
            Data = new Dictionary<string, object>();
        }

        public T GetOrDefault<T>(GameDatum<T> datum, T defaultValue = default)
            => TryGetValue(datum, out var val) ? val : defaultValue;
        public T Get<T>(GameDatum<T> datum)
            => TryGetValue(datum, out var val) ? val : throw new ArgumentException(datum.Name);

        public bool TryGetValue<T>(GameDatum<T> datum, out T value)
        {
            if (Data.TryGetValue(datum.Name, out var obj)) {
                value = (T)obj;
                return true;
            }
            value = default;
            return false;
        }

        public bool TrySetValue<T>(GameDatum<T> datum, T compare, T newValue)
        {
            if (Data.TryGetValue(datum.Name, out var old) && !Equals(old, compare)) {
                return false;
            }
            Data[datum.Name] = newValue;
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
