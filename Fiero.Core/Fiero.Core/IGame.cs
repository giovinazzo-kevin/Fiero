using SFML.Graphics;
using System.Threading;
using System.Threading.Tasks;

namespace Fiero.Core
{
    public interface IGame
    {
        Task RunAsync(CancellationToken ct);
        void Update();
        void Draw();
    }
}
