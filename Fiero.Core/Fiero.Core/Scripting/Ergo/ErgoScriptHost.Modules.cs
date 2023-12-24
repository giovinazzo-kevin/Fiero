using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;

namespace Fiero.Core
{
    public sealed class ErgoModules
    {
        public static readonly Atom Core = new(nameof(Core).ToErgoCase());
    }
}
