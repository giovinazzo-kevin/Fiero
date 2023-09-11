using SFML.Graphics;

namespace Fiero.Core
{
    public interface IGameScene
    {
        object State { get; }
        bool TrySetState(object newState);

        void Update();
        void DrawBackground(RenderTarget target, RenderStates states);
        void DrawForeground(RenderTarget target, RenderStates states);

    }
}
