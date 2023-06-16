namespace Fiero.Core
{
    public interface IGameScene
    {
        object State { get; }
        bool TrySetState(object newState);

        void Update();
        void Draw();

    }
}
