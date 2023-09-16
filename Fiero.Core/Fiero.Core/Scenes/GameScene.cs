using SFML.Graphics;
using System.Collections.Immutable;
using Unconcern.Common;

namespace Fiero.Core
{
    [SingletonDependency]
    public abstract class GameScene<TState> : IGameScene
        where TState : struct, Enum
    {
        private volatile bool _initialized = false;
        object IGameScene.State => State;
        bool IGameScene.TrySetState(object newState) => TrySetState((TState)newState);

        public TState State { get; private set; }

        public event Action<GameScene<TState>, TState> StateChanged;
        private event Action DisposeSubscriptions;
        protected readonly System.Collections.Immutable.ImmutableList<TState> EntryStates;
        protected readonly System.Collections.Immutable.ImmutableList<TState> ExitStates;

        public GameScene()
        {
            EntryStates = ImmutableList.CreateRange(typeof(TState).GetMembers()
                .Where(m => m.GetCustomAttributes(typeof(EntryStateAttribute), false).Any())
                .Select(m => (TState)Enum.Parse(typeof(TState), m.Name))
            );
            ExitStates = ImmutableList.CreateRange(typeof(TState).GetMembers()
                .Where(m => m.GetCustomAttributes(typeof(ExitStateAttribute), false).Any())
                .Select(m => (TState)Enum.Parse(typeof(TState), m.Name))
            );
        }

        public virtual Task InitializeAsync()
        {
            if (_initialized)
            {
                throw new InvalidOperationException("This scene was already initialized");
            }
            _initialized = true;
            return Task.CompletedTask;
        }

        public bool TrySetState(TState newState)
        {
            if (!CanChangeState(newState))
                return false;
            var oldState = State;
            State = newState;
            OnStateChanged(oldState);
            return true;
        }

        protected abstract bool CanChangeState(TState newState);

        private void RerouteEvents(TState newState)
        {
            var isExitState = ExitStates.Contains(newState);
            var isEntryState = EntryStates.Contains(newState);
            if (isEntryState || isExitState)
            {
                DisposeSubscriptions?.Invoke();
            }
            if (isEntryState)
            {
                foreach (var sub in RouteEvents())
                {
                    DisposeSubscriptions += DisposeSubscription;
                    void DisposeSubscription()
                    {
                        sub.Dispose();
                        DisposeSubscriptions -= DisposeSubscription;
                    }
                }
            }
        }

        protected virtual void OnStateChanged(TState oldState)
        {
            RerouteEvents(State);
            StateChanged?.Invoke(this, oldState);
        }

        public virtual IEnumerable<Subscription> RouteEvents()
        {
            yield break;
        }

        public virtual void Update(TimeSpan t, TimeSpan dt) { }
        public virtual void DrawBackground(RenderTarget target, RenderStates states) { }
        public virtual void DrawForeground(RenderTarget target, RenderStates states) { }
    }
}
