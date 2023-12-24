using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;

namespace Fiero.Core
{
    public partial class ErgoScriptHost<TScripts>
        where TScripts : struct, Enum
    {
        public sealed class Modules
        {
            public static readonly Atom Core = new(nameof(Core).ToErgoCase());
        }
    }
}
