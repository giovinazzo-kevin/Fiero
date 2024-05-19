using Ergo.Lang;

namespace Fiero.Core
{

    [Term(Functor = "value", Marshalling = TermMarshalling.Positional)]
    public class UIControlProperty<T> : IUIControlProperty
    {
        [NonTerm]
        private readonly Func<UIControlProperty<T>, UIControlProperty<T>, T, T> _propagate;
        [NonTerm]
        private readonly Func<T, T> _get;
        [NonTerm]
        private readonly Func<T, T> _set;
        [NonTerm]
        public string Name { get; }
        [NonTerm]
        public UIControl Owner { get; private set; }
        [NonTerm]
        public bool Propagated { get; set; }
        [NonTerm]
        public bool Inherited { get; set; }
        [NonTerm]
        public bool Invalidating { get; set; }
        [NonTerm]
        public Type PropertyType { get; } = typeof(T);

        public event Action<UIControlProperty<T>, T> ValueUpdated;
        public event Action<UIControlProperty<T>, T> ValueChanged;
        public event Action<UIControlProperty<T>, UIControl> OwnerChanged;

        public UIControlProperty(
            string name,
            T defaultValue = default,
            Func<UIControlProperty<T>, UIControlProperty<T>, T, T> propagate = null,
            Func<T, T> get = null,
            Func<T, T> set = null,
            bool invalidate = false)
        {
            Name = name;
            _value = defaultValue;
            _propagate = propagate ?? ((a, b, _) => a.V);
            Propagated = propagate != null;
            Inherited = true;
            Invalidating = invalidate;
            _get = get ?? (a => a);
            _set = set ?? (a => a);
        }

        private T _value;
        public T V
        {
            get => _get(_value);
            set
            {
                var old = _value;
                Owner?.OnPropertyChanging(this);
                _value = _set(value);
                ValueUpdated?.Invoke(this, old);
                if (Equals(old, _value))
                {
                    return;
                }
                ValueChanged?.Invoke(this, old);
                if (Propagated && Owner?.Children != null)
                {
                    foreach (var child in Owner.Children)
                    {
                        var prop = child.Properties.SingleOrDefault(p => p.Name.Equals(Name));
                        if (prop is null)
                        {
                            continue;
                        }
                        prop.Value = _propagate(this, prop as UIControlProperty<T>, old);
                    }
                }
            }
        }

        object IUIControlProperty.Value
        {
            get => V;
            set => V = (T)value;
        }

        public void SetOwner(UIControl newOwner)
        {
            var oldOwner = Owner;
            Owner = newOwner;
            OwnerChanged?.Invoke(this, oldOwner);
        }

        public static implicit operator T(UIControlProperty<T> prop) => prop.V;
    }
}
