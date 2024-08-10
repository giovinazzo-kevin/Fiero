using Ergo.Lang.Ast;
using Ergo.Runtime;
using Ergo.Runtime.BuiltIns;

namespace Fiero.Core.Ergo.Libraries.Core.Random;

[SingletonDependency]
public sealed class SetRandomSeed : BuiltIn
{
    public readonly GameDataStore Store;

    public SetRandomSeed(GameDataStore store)
        : base("", new("set_rng_seed"), 1, CoreErgoModules.Random)
    {
        Store = store;
    }

    public override ErgoVM.Op Compile()
    {
        return vm =>
        {
            var arguments = vm.Args;
            if (int.TryParse(arguments[0].Explain(), System.Globalization.NumberStyles.HexNumber, null, out int result))
            {
                Store.SetValue(CoreData.Random.Seed, result);
                Rng.SetGlobalSeed(result);
            }
            else
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, WellKnown.Types.HexString, arguments[0].Explain());
            }
        };
    }
}
