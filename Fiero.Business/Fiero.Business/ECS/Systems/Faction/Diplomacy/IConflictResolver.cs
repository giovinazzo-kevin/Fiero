using System.Numerics;

namespace Fiero.Business
{
    public interface IConflictResolver
    {
        bool TryResolve(ConflictResolutionContext ctx, out Conflict conflict);
    }
}
