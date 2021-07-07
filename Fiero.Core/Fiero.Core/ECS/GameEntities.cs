using LightInject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Fiero.Core
{
    public class GameEntities : IDisposable
    {
        protected class TrackedEntity : IEquatable<TrackedEntity>
        {
            public readonly int Id;
            public List<Component> Components;

            public TrackedEntity(int id)
            {
                Id = id;
                Components = new List<Component>();
            }

            public TrackedEntity(int id, IEnumerable<Component> components)
                : this(id)
            {
                Components.AddRange(components);
            }

            public override bool Equals(object obj) =>  Equals(obj as TrackedEntity);
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
        protected readonly Dictionary<Type, HashSet<PropertyInfo>> ProxyablePropertyCache;
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
            Children = new HashSet<GameEntities>();
            ProtectedEntities = new HashSet<int>();
        }

        public void StartProtecting(int entity) => ProtectedEntities.Add(entity);
        public void StopProtecting(int entity) => ProtectedEntities.Remove(entity);

        public void Clear(bool propagate = true)
        {
            foreach (var e in Entities) {
                FlagEntityForRemoval(e.Id);
            }
            RemoveFlaggedItems(propagate);
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
            where T : Entity
        {
            if(!TryGetProxy<T>(entityId, out var entity)) {
                throw new ArgumentException($"A proxy for type {typeof(T).Name} could not be created");
            }
            return entity;
        }

        public IEnumerable<PropertyInfo> GetProxyableProperties<T>()
            where T: Entity
        {
            if (!ProxyablePropertyCache.TryGetValue(typeof(T), out var props)) {
                ProxyablePropertyCache[typeof(T)] = props = typeof(T).GetProperties()
                    .Where(p => p.PropertyType.IsAssignableTo(typeof(Component)))
                    .Select(p => p.DeclaringType.GetProperty(p.Name))
                    .ToHashSet();
            }
            return props;
        }

        public EntityBuilder<T> CreateBuilder<T>() where T : Entity => new(this);

        public bool TryGetProxy<T>(int entityId, out T entity)
            where T : Entity
        {
            var equalEntity = new TrackedEntity(entityId);
            if (!Entities.TryGetValue(equalEntity, out var trackedEntity)) {
                throw new ArgumentOutOfRangeException($"An entity with id {entityId} is not being tracked");
            }
            var props = GetProxyableProperties<T>();
            entity = ServiceFactory.GetInstance<T>();
            entity._refresh = (entity, entityId) => {
                entity.Id = entityId <= 0 ? 0 : entityId;
                if (EntityRemovalQueue.Contains(entityId)) {
                    return false;
                }
                foreach (var p in props) {
                    var comp = trackedEntity.Components.FirstOrDefault(c => c.GetType() == p.PropertyType);
                    if (comp == null && p.GetCustomAttribute<RequiredComponentAttribute>() is { }) {
                        return false;
                    }
                    if(entityId > 0) {
                        p.SetValue(entity, comp, BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);
                    }
                    else {
                        p.SetValue(entity, null, BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);
                    }
                }
                return true;
            };
            return entity.TryRefresh(entityId);
        }


        public IEnumerable<int> GetEntities() => Entities.Select(e => e.Id);

        public bool FlagEntityForRemoval(int entityId)
        {
            if (ProtectedEntities.Contains(entityId))
                return false;

            var equalEntity = new TrackedEntity(entityId);
            if (!Entities.TryGetValue(equalEntity, out var trackedEntity) || EntityRemovalQueue.Contains(equalEntity.Id)) {
                return false;
            }
            EntityRemovalQueue.Enqueue(equalEntity.Id);
            foreach (var component in trackedEntity.Components) {
                if(!FlagComponentForRemoval(trackedEntity.Id, component.Id)) {
                    throw new InvalidOperationException($"Component cache is in an invalid state");
                }
            }
            return Parent?.FlagEntityForRemoval(entityId) ?? true;
        }
        public void AddComponent<TComponent>(int entityId, Func<TComponent, TComponent> initialize = null)
            where TComponent : Component
        {
            var equalEntity = new TrackedEntity(entityId);
            if (!Entities.TryGetValue(equalEntity, out var trackedEntity)) {
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
            if(!Components.TryGetValue(tComponent, out var hashSet)) {
                Components[tComponent] = hashSet = new HashSet<TrackedEntity>();
            }
            if(Parent != null) {
                if (!Parent.Components.TryGetValue(tComponent, out var parentHashSet)) {
                    Parent.Components[tComponent] = parentHashSet = new HashSet<TrackedEntity>();
                }
                parentHashSet.Add(trackedEntity);
            }
            hashSet.Add(trackedEntity);
        }

        public bool FlagComponentForRemoval(int entityId, int componentId)
        {
            var equalEntity = new TrackedEntity(entityId);
            if (!Entities.TryGetValue(equalEntity, out var trackedEntity)) {
                return false;
            }
            if (!trackedEntity.Components.Exists(c => c.Id == componentId)) {
                return false;
            }
            ComponentRemovalQueue.Enqueue((entityId, componentId));
            return Parent?.FlagComponentForRemoval(entityId, componentId) ?? true;
        }

        public void RemoveFlaggedItems(bool propagate = true) // Don't call while enumerating, obviously
        {
            while(ComponentRemovalQueue.TryDequeue(out (int EntityId, int ComponentId) tup)) {
                var equalEntity = new TrackedEntity(tup.EntityId);
                if (!Entities.TryGetValue(equalEntity, out var trackedEntity)) {
                    continue;
                }
                if (!(trackedEntity.Components.FirstOrDefault(c => c.Id == tup.ComponentId) is { } component)) {
                    continue;
                }
                var tComponent = component.GetType();
                if (!Components.TryGetValue(tComponent, out var hashSet)) {
                    throw new InvalidOperationException($"Component cache is in an invalid state");
                }
                trackedEntity.Components.Remove(component);
                hashSet.Remove(trackedEntity);
                if (hashSet.Count == 0) {
                    Components.Remove(tComponent);
                }
            }
            while (EntityRemovalQueue.TryDequeue(out var entityId)) {
                var equalEntity = new TrackedEntity(entityId);
                if (!Entities.TryGetValue(equalEntity, out var trackedEntity) || EntityRemovalQueue.Contains(equalEntity.Id)) {
                    continue;
                }
                if (trackedEntity.Components.Count > 0) {
                    throw new InvalidOperationException($"Component cache is in an invalid state");
                }
                Entities.Remove(trackedEntity);
            }
            if(propagate) {
                Parent?.RemoveFlaggedItems();
            }
        }

        public void Update<TComponent>(int entityId, Action<TComponent> update)
            where TComponent : Component
        {
            if (EntityRemovalQueue.Contains(entityId))
                return;
            // Components being tracked by parent containers are automatically updated due to them being reference types
            var equalEntity = new TrackedEntity(entityId);
            if (!Entities.TryGetValue(equalEntity, out var trackedEntity)) {
                throw new ArgumentOutOfRangeException($"An entity with id {entityId} is not being tracked");
            }
            foreach (var comp in trackedEntity.Components.OfType<TComponent>()) {
                update(comp);
            }
        }

        public IEnumerable<TComponent> GetComponents<TComponent>()
        {
            var tComponent = typeof(TComponent);
            if (Components.TryGetValue(tComponent, out var entities)) {
                return entities.SelectMany(e => e.Components.OfType<TComponent>());
            }
            return Enumerable.Empty<TComponent>();
        }



        public bool TryGetFirstComponent<TComponent>(int entityId, out TComponent component)
        {
            if(GetComponents(entityId).OfType<TComponent>().FirstOrDefault() is { } c) {
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

        public IEnumerable<Component> GetComponents(int entityId)
        {
            if (EntityRemovalQueue.Contains(entityId))
                return Enumerable.Empty<Component>();
            var equalEntity = new TrackedEntity(entityId);
            if (!Entities.TryGetValue(equalEntity, out var trackedEntity)) {
                throw new ArgumentOutOfRangeException($"An entity with id {entityId} is not being tracked");
            }
            return trackedEntity.Components;
        }

        private volatile bool _diposed;
        public void Dispose()
        {
            if(_diposed) {
                throw new ObjectDisposedException(nameof(GameEntities));
            }
            if(ProtectedEntities.Any()) {
                throw new InvalidOperationException("Cannot dispose GameEntities while protected entities still exist");
            }
            foreach (var e in Entities) {
                FlagEntityForRemoval(e.Id);
            }
            RemoveFlaggedItems(propagate: true);
            _diposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
