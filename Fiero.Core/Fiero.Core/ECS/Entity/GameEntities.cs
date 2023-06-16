using Fiero.Core.Structures;
using System.Reflection;

namespace Fiero.Core
{
    public class GameEntities : IDisposable
    {
        protected class TrackedEntity : IEquatable<TrackedEntity>
        {
            public readonly int Id;
            public List<EcsComponent> Components;

            public TrackedEntity(int id)
            {
                Id = id;
                Components = new List<EcsComponent>();
            }

            public TrackedEntity(int id, IEnumerable<EcsComponent> components)
                : this(id)
            {
                Components.AddRange(components);
            }

            public override bool Equals(object obj) => Equals(obj as TrackedEntity);
            public bool Equals(TrackedEntity other) => Id == other?.Id;
            public override int GetHashCode() => Id;
        }


        // Must: create/destroy entities, store them in an indexed collection HashSet<Entity>
        //       add/remove components to entities, store them in another indexed collection Dict<TComponent, HashSet<Entity>>

        protected volatile int _lastEntityId = 0;
        protected volatile int _lastComponentId = 0;

        public readonly IServiceFactory ServiceFactory;
        protected readonly HashSet<TrackedEntity> Entities;
        protected readonly Dictionary<Type, HashSet<TrackedEntity>> Components;
        protected readonly Dictionary<int, EcsComponent> ComponentsLookup;
        protected readonly Dictionary<Type, HashSet<PropertyInfo>> ProxyablePropertyCache;
        protected readonly Dictionary<OrderedPair<int, Type>, EcsEntity> ProxyCache;
        protected readonly Queue<int> EntityRemovalQueue;
        protected readonly Queue<(int Entity, int Component)> ComponentRemovalQueue;
        protected readonly HashSet<int> ProtectedEntities;

        protected readonly GameEntities Parent;
        protected readonly HashSet<GameEntities> Children;

        public GameEntities CreateScope()
        {
            return new GameEntities(ServiceFactory, this);
        }

        private GameEntities(IServiceFactory factory, GameEntities parent)
            : this(factory)
        {
            Parent = parent;
            parent.Children.Add(this);
        }

        public GameEntities(IServiceFactory serviceFactory)
        {
            Parent = null;
            ServiceFactory = serviceFactory;
            Entities = new HashSet<TrackedEntity>();
            Components = new Dictionary<Type, HashSet<TrackedEntity>>();
            EntityRemovalQueue = new Queue<int>();
            ComponentRemovalQueue = new Queue<(int Entity, int Component)>();
            ProxyablePropertyCache = new Dictionary<Type, HashSet<PropertyInfo>>();
            ProxyCache = new Dictionary<OrderedPair<int, Type>, EcsEntity>();
            Children = new HashSet<GameEntities>();
            ProtectedEntities = new HashSet<int>();
            ComponentsLookup = new Dictionary<int, EcsComponent>();
        }

        public void StartProtecting(int entity) => ProtectedEntities.Add(entity);
        public void StopProtecting(int entity) => ProtectedEntities.Remove(entity);

        public void Clear(bool propagate = true)
        {
            foreach (var e in Entities)
            {
                FlagEntityForRemoval(e.Id);
            }
            RemoveFlagged(propagate);
            _lastComponentId = 0;
            _lastEntityId = 0;
        }

        public int CreateEntity()
        {
            var entity = Parent != null
                ? (_lastEntityId = Interlocked.Increment(ref Parent._lastEntityId))
                : Interlocked.Increment(ref _lastEntityId);
            var trackedEntity = new TrackedEntity(entity);
            Entities.Add(trackedEntity);
            Parent?.Entities.Add(trackedEntity);
            return entity;
        }

        public T GetProxy<T>(int entityId)
            where T : EcsEntity
        {
            if (!TryGetProxy<T>(entityId, out var entity))
            {
                throw new ArgumentException($"A proxy for type {typeof(T).Name} could not be created");
            }
            return entity;
        }

        public IEnumerable<PropertyInfo> GetProxyableProperties<T>()
            where T : EcsEntity
        {
            if (!ProxyablePropertyCache.TryGetValue(typeof(T), out var props))
            {
                ProxyablePropertyCache[typeof(T)] = props = typeof(T).GetProperties()
                    .Where(p => p.PropertyType.IsAssignableTo(typeof(EcsComponent)))
                    .Select(p => p.DeclaringType.GetProperty(p.Name))
                    .ToHashSet();
            }
            return props;
        }

        public EntityBuilder<T> CreateBuilder<T>() where T : EcsEntity => new(this);

        public bool TryGetProxy<T>(int entityId, out T entity)
            where T : EcsEntity
        {
            entity = default;
            if (entityId == 0)
                return false;
            var equalEntity = new TrackedEntity(entityId);
            var cacheKey = new OrderedPair<int, Type>(entityId, typeof(T));
            if (ProxyCache.TryGetValue(cacheKey, out var proxy))
            {
                if (!Entities.TryGetValue(equalEntity, out _))
                {
                    ProxyCache.Remove(cacheKey);
                    return false;
                }
                entity = (T)proxy;
                return true;
            }
            if (!Entities.TryGetValue(equalEntity, out var trackedEntity))
            {
                return false;
            }
            var props = GetProxyableProperties<T>();
            entity = ServiceFactory.GetInstance<T>();
            entity._refresh = (entity, entityId) =>
            {
                entity.Id = entityId <= 0 ? 0 : entityId;
                if (EntityRemovalQueue.Contains(entityId))
                {
                    return false;
                }
                foreach (var p in props)
                {
                    var comp = trackedEntity.Components.FirstOrDefault(c => c.GetType() == p.PropertyType);
                    if (comp == null && p.GetCustomAttribute<RequiredComponentAttribute>() is { })
                    {
                        return false;
                    }
                    if (entityId > 0)
                    {
                        p.SetValue(entity, comp, BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);
                    }
                    else
                    {
                        p.SetValue(entity, null, BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);
                    }
                }
                return true;
            };
            entity._cast = (entity, type) =>
            {
                var tryGetProxy = typeof(GameEntities)
                    .GetMethod(nameof(TryGetProxy))
                    .MakeGenericMethod(type);
                var proxiedEntity = Activator.CreateInstance(type); proxiedEntity = null;
                object[] args = new object[] { entity.Id, proxiedEntity };
                var ret = (bool)tryGetProxy.Invoke(this, args);
                if (ret)
                {
                    var retEnt = (EcsEntity)args[1];
                    return retEnt;
                }
                return null;
            };
            if (entity.TryRefresh(entityId))
            {
                ProxyCache[cacheKey] = entity;
                return true;
            }
            return false;
        }


        public IEnumerable<int> GetEntities() => Entities.Select(e => e.Id);

        public bool IsEntityFlaggedForRemoval(int entityId)
        {
            return EntityRemovalQueue.Contains(entityId);
        }

        public bool FlagEntityForRemoval(int entityId)
        {
            if (ProtectedEntities.Contains(entityId))
                return false;

            var equalEntity = new TrackedEntity(entityId);
            if (!Entities.TryGetValue(equalEntity, out var trackedEntity) || EntityRemovalQueue.Contains(equalEntity.Id))
            {
                return false;
            }
            EntityRemovalQueue.Enqueue(equalEntity.Id);
            foreach (var component in trackedEntity.Components)
            {
                if (!FlagComponentForRemoval(trackedEntity.Id, component.Id))
                {
                    throw new InvalidOperationException($"Component cache is in an invalid state");
                }
            }
            return Parent?.FlagEntityForRemoval(entityId) ?? true;
        }
        public void AddComponent<TComponent>(int entityId, Func<TComponent, TComponent> initialize = null)
            where TComponent : EcsComponent
        {
            var equalEntity = new TrackedEntity(entityId);
            if (!Entities.TryGetValue(equalEntity, out var trackedEntity))
            {
                throw new ArgumentOutOfRangeException($"An entity with id {entityId} is not being tracked");
            }
            var component = ServiceFactory.GetInstance<TComponent>();
            component.Id = Parent != null
                ? (_lastComponentId = Interlocked.Increment(ref _lastComponentId))
                : Interlocked.Increment(ref _lastComponentId);
            component.EntityId = entityId;
            initialize?.Invoke(component);
            trackedEntity.Components.Add(component);
            var tComponent = typeof(TComponent);
            if (!Components.TryGetValue(tComponent, out var hashSet))
            {
                Components[tComponent] = hashSet = new HashSet<TrackedEntity>();
            }
            if (Parent != null)
            {
                if (!Parent.Components.TryGetValue(tComponent, out var parentHashSet))
                {
                    Parent.Components[tComponent] = parentHashSet = new HashSet<TrackedEntity>();
                }
                parentHashSet.Add(trackedEntity);
            }
            hashSet.Add(trackedEntity);
            ComponentsLookup[component.Id] = component;
        }

        public bool FlagComponentForRemoval(int entityId, int componentId)
        {
            var equalEntity = new TrackedEntity(entityId);
            if (!Entities.TryGetValue(equalEntity, out var trackedEntity))
            {
                return false;
            }
            if (!trackedEntity.Components.Exists(c => c.Id == componentId))
            {
                return false;
            }
            ComponentRemovalQueue.Enqueue((entityId, componentId));
            return Parent?.FlagComponentForRemoval(entityId, componentId) ?? true;
        }

        public void RemoveFlagged(bool propagate = true) // Don't call while enumerating, obviously
        {
            while (ComponentRemovalQueue.TryDequeue(out (int EntityId, int ComponentId) tup))
            {
                var equalEntity = new TrackedEntity(tup.EntityId);
                if (!Entities.TryGetValue(equalEntity, out var trackedEntity))
                {
                    continue;
                }
                if (!(trackedEntity.Components.FirstOrDefault(c => c.Id == tup.ComponentId) is { } component))
                {
                    continue;
                }
                var tComponent = component.GetType();
                if (!Components.TryGetValue(tComponent, out var hashSet))
                {
                    throw new InvalidOperationException($"Component cache is in an invalid state");
                }
                trackedEntity.Components.Remove(component);
                hashSet.Remove(trackedEntity);
                if (hashSet.Count == 0)
                {
                    Components.Remove(tComponent);
                }
                ComponentsLookup.Remove(component.Id);
            }
            while (EntityRemovalQueue.TryDequeue(out var entityId))
            {
                var equalEntity = new TrackedEntity(entityId);
                if (!Entities.TryGetValue(equalEntity, out var trackedEntity) || EntityRemovalQueue.Contains(equalEntity.Id))
                {
                    continue;
                }
                if (trackedEntity.Components.Count > 0)
                {
                    throw new InvalidOperationException($"Component cache is in an invalid state");
                }
                Entities.Remove(trackedEntity);
                foreach (var key in ProxyCache.Keys.Where(k => k.Left == entityId).ToArray())
                {
                    ProxyCache.Remove(key);
                }
            }
            if (propagate)
            {
                Parent?.RemoveFlagged();
            }
        }


        public void Update<TComponent>(int entityId, TComponent copyProperties)
            where TComponent : EcsComponent
            => Update<TComponent>(entityId, comp =>
        {

        });

        public void Update<TComponent>(int entityId, Action<TComponent> update)
            where TComponent : EcsComponent
        {
            if (EntityRemovalQueue.Contains(entityId))
                return;
            // Components being tracked by parent containers are automatically updated due to them being reference types
            var equalEntity = new TrackedEntity(entityId);
            if (!Entities.TryGetValue(equalEntity, out var trackedEntity))
            {
                throw new ArgumentOutOfRangeException($"An entity with id {entityId} is not being tracked");
            }
            foreach (var comp in trackedEntity.Components.OfType<TComponent>())
            {
                update(comp);
            }
        }

        public IEnumerable<TComponent> GetComponents<TComponent>()
        {
            var tComponent = typeof(TComponent);
            if (Components.TryGetValue(tComponent, out var entities))
            {
                return entities.SelectMany(e => e.Components.OfType<TComponent>());
            }
            return Enumerable.Empty<TComponent>();
        }

        public bool TryGetComponent<TComponent>(int id, out TComponent component)
        {
            if (ComponentsLookup.TryGetValue(id, out var comp) && comp is TComponent comp_)
            {
                component = comp_;
                return true;
            }
            component = default;
            return false;
        }

        public bool TryGetFirstComponent<TComponent>(int entityId, out TComponent component)
        {
            if (GetComponents(entityId).OfType<TComponent>().FirstOrDefault() is { } c)
            {
                component = c;
                return true;
            }
            component = default;
            return false;
        }

        public TComponent GetFirstComponent<TComponent>(int entityId)
            => GetComponents(entityId).OfType<TComponent>().First();

        public TComponent GetSingleComponent<TComponent>(int entityId)
            => GetComponents(entityId).OfType<TComponent>().Single();

        public bool IsTracking(int entityId) => Entities.TryGetValue(new TrackedEntity(entityId), out _);

        public IEnumerable<EcsComponent> GetComponents(int entityId)
        {
            if (EntityRemovalQueue.Contains(entityId))
                return Enumerable.Empty<EcsComponent>();
            var equalEntity = new TrackedEntity(entityId);
            if (!Entities.TryGetValue(equalEntity, out var trackedEntity))
            {
                throw new ArgumentOutOfRangeException($"An entity with id {entityId} is not being tracked");
            }
            return trackedEntity.Components;
        }

        private volatile bool _diposed;
        public void Dispose()
        {
            if (_diposed)
            {
                throw new ObjectDisposedException(nameof(GameEntities));
            }
            if (ProtectedEntities.Any())
            {
                throw new InvalidOperationException("Cannot dispose GameEntities while protected entities still exist");
            }
            foreach (var e in Entities)
            {
                FlagEntityForRemoval(e.Id);
            }
            RemoveFlagged(propagate: true);
            _diposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
