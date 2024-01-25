using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using System.Reflection;

namespace Fiero.Core
{
    public interface IEntityBuilder
    {
        EcsEntity Build();
    }

    public interface IEntityBuilder<out TProxy> : IEntityBuilder
        where TProxy : EcsEntity
    {
    }
    public sealed class EntityBuilder<TProxy> : IEntityBuilder<TProxy>
        where TProxy : EcsEntity
    {
        private readonly ImmutableHashSet<Type> _componentTypes;
        private Action<int> _configure;

        public event Action<EntityBuilder<TProxy>, TProxy> Built;
        public readonly GameEntities Entities;

        private static readonly MethodInfo __AddOrTweak = typeof(EntityBuilder<TProxy>).GetMethod(nameof(AddOrTweak));

        internal EntityBuilder(GameEntities entities, IEnumerable<Type> compTypes = null, Action<int> configure = null)
        {
            _configure = configure ?? (_ => { });
            Entities = entities;
            _componentTypes = ImmutableHashSet.CreateRange(compTypes ?? Enumerable.Empty<Type>());
        }

        public EntityBuilder<TProxy> Add<T>(Action<IServiceFactory, T> configure = null)
            where T : EcsComponent
        {
            if (_componentTypes.Contains(typeof(T)))
            {
                throw new ArgumentException($"EntityBuilder for entity proxy of type {typeof(TProxy).Name} already has a component of type {typeof(T).Name}");
            }
            var proxyableComps = Entities.GetProxyableProperties<TProxy>()
                .Select(p => p.PropertyType);
            if (!proxyableComps.Contains(typeof(T)))
            {
                throw new ArgumentException($"Entity proxy of type {typeof(TProxy).Name} has no component definition of type {typeof(T).Name}");
            }
            var builder = new EntityBuilder<TProxy>(
                Entities,
                _componentTypes.Add(typeof(T)),
                e =>
                {
                    _configure(e);
                    Entities.AddComponent<T>(e, c => { configure?.Invoke(Entities.ServiceFactory, c); return c; });
                }
            );
            builder.Built += (b, e) => Built?.Invoke(b, e);
            return builder;
        }

        public EntityBuilder<TProxy> Tweak<T>(Action<IServiceFactory, T> configure)
            where T : EcsComponent
        {
            var x = typeof(DBNull);


            if (!_componentTypes.Contains(typeof(T)))
            {
                throw new ArgumentException($"EntityBuilder for entity of type {typeof(TProxy).Name} has no component definition of type {typeof(T).Name}");
            }
            var builder = new EntityBuilder<TProxy>(
                Entities,
                _componentTypes,
                e =>
                {
                    _configure(e);
                    configure?.Invoke(Entities.ServiceFactory, Entities.GetFirstComponent<T>(e));
                }
            );
            builder.Built += (b, e) => Built?.Invoke(b, e);
            return builder;
        }

        public EntityBuilder<TProxy> AddOrTweak<T>(Action<IServiceFactory, T> configure = null)
            where T : EcsComponent
        {
            if (!_componentTypes.Contains(typeof(T)))
            {
                return Add(configure);
            }
            return Tweak(configure);
        }

        public EntityBuilder<TProxy> Load(Type type, Dict from)
        {
            var ret = (EntityBuilder<TProxy>)__AddOrTweak.MakeGenericMethod([type]).Invoke(this, [null]);
            var props = type.GetProperties()
                .ToDictionary(x => new Atom(x.Name.ToErgoCase()));
            var kvps = from.KeyValuePairs
                .ToDictionary(a => (Atom)((Complex)a).Arguments[0], a => ((Complex)a).Arguments[1]);
            var values = new Dictionary<Atom, object>();
            foreach (var (key, prop) in props)
            {
                if (kvps.TryGetValue(key, out var value))
                    values.Add(key, TermMarshall.FromTerm(value, prop.PropertyType));
            }
            var builder = new EntityBuilder<TProxy>(
                Entities,
                ret._componentTypes,
                e =>
                {
                    ret._configure(e);
                    var to = Entities.GetComponents(e)
                        .Single(x => x.GetType().Equals(type));
                    foreach (var (key, val) in values)
                        props[key].SetValue(to, val);
                }
            );
            return builder;
        }

        public TProxy Build()
        {
            var e = Entities.CreateEntity();
            _configure(e);
            if (!Entities.TryGetProxy(e, out TProxy proxy))
            {
                throw new ArgumentException($"Could not build a proxy for entity type {typeof(TProxy).Name} out of components {String.Join(", ", _componentTypes.Select(x => x.Name))}");
            }
            proxy._clone = Build;
            var cast = proxy._cast;
            proxy._cast = (e, t) =>
            {
                var r = cast(e, t);
                if (r != null)
                {
                    r._clone = Build;
                }
                return r;
            };
            Built?.Invoke(this, proxy);
            return proxy;
        }

        EcsEntity IEntityBuilder.Build() => Build();
    }
}
