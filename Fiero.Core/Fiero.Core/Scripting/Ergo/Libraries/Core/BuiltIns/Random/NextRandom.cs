using Ergo.Lang.Ast;
using Ergo.Runtime;
using Ergo.Runtime.BuiltIns;

namespace Fiero.Core.Ergo.Libraries.Core.Random;

[SingletonDependency]
public sealed class NextRandom : BuiltIn
{
    public NextRandom()
        : base("", new("rng"), 1, CoreErgoModules.Random)
    {
    }

    public override ErgoVM.Op Compile()
    {
        return vm =>
        {
            var N = Rng.Random.NextDouble();
            vm.SetArg(1, new Atom(N));
            ErgoVM.Goals.Unify2(vm);
        };
    }
}
