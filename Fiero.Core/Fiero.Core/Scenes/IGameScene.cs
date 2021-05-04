using SFML.Graphics;

namespace Fiero.Core
{
    public interface IGameScene
    {
        object State { get; }
        bool TrySetState(object newState);

        void Update(RenderWindow win, float t, float dt);
        void Draw(RenderWindow win, float t, float dt);

    }
}
