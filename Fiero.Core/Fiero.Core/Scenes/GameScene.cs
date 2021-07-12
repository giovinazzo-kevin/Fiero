using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unconcern.Common;

namespace Fiero.Core
{
    [SingletonDependency]
    public abstract class GameScene<TState> : IGameScene
        where TState : struct, Enum
    {
        private volatile bool _initialized = false;
        object IGameScene.State => State;
        Task<bool> IGameScene.TrySetStateAsync(object newState) => TrySetStateAsync((TState)newState);

        public TState State { get; private set; }

        public event Action<GameScene<TState>, TState> StateChanged;
        private event Action DisposeSubscriptions;
        protected readonly ImmutableList<TState> EntryStates;
        protected readonly ImmutableList<TState> ExitStates;

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
            if(_initialized) {
                throw new InvalidOperationException("This scene was already initialized");
            }
            _initialized = true;
            return Task.CompletedTask;
        }

        public async Task<bool> TrySetStateAsync(TState newState)
        {
            if (!await CanChangeStateAsync(newState))
                return false;
            var oldState = State;
            State = newState;
            await OnStateChangedAsync(oldState);
            return true;
        }

        protected abstract Task<bool> CanChangeStateAsync(TState newState);
        protected virtual async Task OnStateChangedAsync(TState oldState)
        {
            StateChanged?.Invoke(this, oldState);
            if (ExitStates.Contains(State)) {
                DisposeSubscriptions?.Invoke();
            }

            if (EntryStates.Contains(State)) {
                await foreach (var sub in RouteEventsAsync()) {
                    Action dispose = async () => {
                        await sub.DisposeAsync();
                    };
                    dispose += () => DisposeSubscriptions -= dispose;
                    DisposeSubscriptions += dispose;
                }
            }
        }

        public virtual async IAsyncEnumerable<Subscription> RouteEventsAsync()
        {
            await Task.CompletedTask;
            yield break;
        }

        public virtual void Update(RenderWindow win, float t, float dt) { }
        public virtual void Draw(RenderWindow win, float t, float dt) { }
    }
}
