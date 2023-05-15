using LightInject;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Fiero.Core
{
    public interface IEntityBuilder
    {
        EcsEntity Build();
    }

    public sealed class EntityBuilder<TProxy> : IEntityBuilder
        where TProxy : EcsEntity
    {
        private readonly ImmutableHashSet<Type> _componentTypes;
        private Action<int> _configure;
        private readonly GameEntities _entities;

        public IServiceFactory ServiceFactory => _entities.ServiceFactory;
        public event Action<EntityBuilder<TProxy>, TProxy> Built;

        internal EntityBuilder(GameEntities entities, IEnumerable<Type> compTypes = null, Action<int> configure = null)
        {
            _configure = configure ?? (_ => { });
            _entities = entities;
            _componentTypes = ImmutableHashSet.CreateRange(compTypes ?? Enumerable.Empty<Type>());
        }

        public EntityBuilder<TProxy> Add<T>(Action<T> configure = null)
            where T : EcsComponent
        {
            if (_componentTypes.Contains(typeof(T)))
            {
                throw new ArgumentException($"EntityBuilder for entity proxy of type {typeof(TProxy).Name} already has a component of type {typeof(T).Name}");
            }
            var proxyableComps = _entities.GetProxyableProperties<TProxy>()
                .Select(p => p.PropertyType);
            if (!proxyableComps.Contains(typeof(T)))
            {
                throw new ArgumentException($"Entity proxy of type {typeof(TProxy).Name} has no component definition of type {typeof(T).Name}");
            }
            var builder = new EntityBuilder<TProxy>(
                _entities,
                _componentTypes.Add(typeof(T)),
                e =>
                {
                    _configure(e);
                    _entities.AddComponent<T>(e, c => { configure?.Invoke(c); return c; });
                }
            );
            builder.Built += (b, e) => Built?.Invoke(b, e);
            return builder;
        }

        public EntityBuilder<TProxy> Tweak<T>(Action<T> configure)
            where T : EcsComponent
        {
            if (!_componentTypes.Contains(typeof(T)))
            {
                throw new ArgumentException($"EntityBuilder for entity of type {typeof(TProxy).Name} has no component definition of type {typeof(T).Name}");
            }
            var builder = new EntityBuilder<TProxy>(
                _entities,
                _componentTypes,
                e =>
                {
                    _configure(e);
                    configure?.Invoke(_entities.GetFirstComponent<T>(e));
                }
            );
            builder.Built += (b, e) => Built?.Invoke(b, e);
            return builder;
        }

        public EntityBuilder<TProxy> AddOrTweak<T>(Action<T> configure = null)
            where T : EcsComponent
        {
            if (!_componentTypes.Contains(typeof(T)))
            {
                return Add(configure);
            }
            return Tweak(configure);
        }

        public TProxy Build()
        {
            var e = _entities.CreateEntity();
            _configure(e);
            if (!_entities.TryGetProxy(e, out TProxy proxy))
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
