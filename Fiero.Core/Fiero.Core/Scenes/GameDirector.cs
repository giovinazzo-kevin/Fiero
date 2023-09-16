using SFML.Graphics;

namespace Fiero.Core
{
    public class GameDirector
    {
        internal class CachedItem
        {
            public readonly Type TState;
            public readonly IGameScene Scene;
            public readonly Dictionary<object, object> Transitions;

            public CachedItem(Type state, IGameScene scene)
            {
                TState = state;
                Scene = scene;
                Transitions = new Dictionary<object, object>();
            }
        }

        private readonly Dictionary<Type, CachedItem> _cache;
        private readonly Queue<CachedItem> _transitions;
        private CachedItem _currentItem;

        public IGameScene CurrentScene => _currentItem?.Scene;

        public GameDirector()
        {
            _cache = new Dictionary<Type, CachedItem>();
            _transitions = new Queue<CachedItem>();
        }

        public bool TrySetState<T>(T state)
            where T : struct, Enum
        {
            var tState = typeof(T);
            if (!_cache.TryGetValue(tState, out var cached))
            {
                throw new InvalidOperationException($"A scene with state {tState.Name} was not registered");
            }
            if (_currentItem != null)
            {
                if (_currentItem.Scene.TrySetState(state))
                {
                    return true;
                }
                return false;
            }
            _currentItem = cached;
            return true;
        }

        public async Task AddScene<T>(GameScene<T> scene)
            where T : struct, Enum
        {
            var tState = typeof(T);
            if (_cache.ContainsKey(tState))
            {
                throw new InvalidOperationException($"A scene with state {tState.Name} was already registered");
            }
            _cache[tState] = new CachedItem(tState, scene);
            await scene.InitializeAsync();
        }

        public async Task AddScenes(IEnumerable<IGameScene> scenes)
        {
            var addSceneGeneric = typeof(GameDirector).GetMethod(nameof(AddScene));
            foreach (var scene in scenes)
            {
                var addScene = addSceneGeneric.MakeGenericMethod(scene.State.GetType());
                await (Task)addScene.Invoke(this, new object[] { scene });
            }
        }

        public void MapTransition<A, B>(A fromState, B toState)
            where A : struct, Enum
            where B : struct, Enum
        {
            var tStateA = typeof(A);
            var tStateB = typeof(B);
            if (!_cache.TryGetValue(tStateA, out var a))
            {
                throw new InvalidOperationException($"A scene with state {tStateA.Name} was not registered");
            }
            if (!_cache.ContainsKey(tStateB))
            {
                throw new InvalidOperationException($"A scene with state {tStateB.Name} was not registered");
            }
            if (!a.Transitions.TryAdd(fromState, toState))
            {
                throw new InvalidOperationException($"A transition from state {tStateA.Name} to state {tStateB.Name} was already registered");
            }
        }

        public virtual void Update(TimeSpan t, TimeSpan dt)
        {
            if (_currentItem != null)
            {
                _currentItem.Scene.Update(t, dt);
                if (_currentItem.Transitions.TryGetValue(_currentItem.Scene.State, out var nextState))
                {
                    var tNextState = nextState.GetType();
                    if (!_cache.TryGetValue(tNextState, out var nextItem))
                    {
                        throw new InvalidOperationException($"Could not access cached item for transition from  state {_currentItem.Scene.State.GetType().Name} to state {tNextState.Name}");
                    }
                    if (nextItem.Scene.TrySetState(nextState))
                    {
                        _currentItem = nextItem;
                    }
                }
            }
        }

        public virtual void DrawBackground(RenderTarget target, RenderStates states)
        {
            if (_currentItem != null)
            {
                _currentItem.Scene.DrawBackground(target, states);
            }
        }

        public virtual void DrawForeground(RenderTarget target, RenderStates states)
        {
            if (_currentItem != null)
            {
                _currentItem.Scene.DrawForeground(target, states);
            }
        }
    }
}
