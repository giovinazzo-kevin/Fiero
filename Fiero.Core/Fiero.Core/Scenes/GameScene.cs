using SFML.Graphics;
using System;
using System.Text;

namespace Fiero.Core
{

    public abstract class GameScene<TState> : IGameScene
        where TState : struct, Enum
    {
        private volatile bool _initialized = false;
        object IGameScene.State => State;
        bool IGameScene.TrySetState(object newState) => TrySetState((TState)newState);

        public TState State { get; private set; }

        public GameScene()
        {
        }

        public virtual void Initialize()
        {
            if(_initialized) {
                throw new InvalidOperationException("This scene was already initialized");
            }
            _initialized = true;
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
        protected abstract void OnStateChanged(TState oldState);

        public virtual void Update(RenderWindow win, float t, float dt) { }
        public virtual void Draw(RenderWindow win, float t, float dt) { }
    }
}
