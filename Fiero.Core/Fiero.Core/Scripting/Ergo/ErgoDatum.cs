using Ergo.Lang.Ast;

namespace Fiero.Core
{
    public sealed class ErgoDatum(string module, string name) : GameDatum<ITerm>(module, name, isStatic: false)
    {
    }
}
