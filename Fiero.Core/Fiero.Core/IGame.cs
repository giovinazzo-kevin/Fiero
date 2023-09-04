
using SFML.Graphics;

namespace Fiero.Core;

public interface IGame
{
    Task RunAsync(CancellationToken ct);
    void Update();
    void Draw(RenderTarget target, RenderStates states);
}
