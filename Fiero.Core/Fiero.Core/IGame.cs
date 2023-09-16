
using SFML.Graphics;

namespace Fiero.Core;

public interface IGame
{
    Task RunAsync(CancellationToken ct);
    void Update(TimeSpan t, TimeSpan dt);
    void Draw(RenderTarget target, RenderStates states);
}
