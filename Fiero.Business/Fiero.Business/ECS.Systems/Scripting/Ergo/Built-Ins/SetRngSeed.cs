using Ergo.Lang.Ast;
using Ergo.Lang.Exceptions;
using Ergo.Solver;
using Ergo.Solver.BuiltIns;

namespace Fiero.Business;

[SingletonDependency]
public sealed class SetRngSeed : SolverBuiltIn
{
    public readonly GameDataStore Store;

    public SetRngSeed(GameDataStore store)
        : base("", new("set_rng_seed"), 1, ErgoScriptingSystem.FieroModule)
    {
        Store = store;
    }

    public override IEnumerable<Evaluation> Apply(SolverContext solver, SolverScope scope, ITerm[] arguments)
    {
        if (int.TryParse(arguments[0].Explain(), System.Globalization.NumberStyles.HexNumber, null, out int result))
        {
            Store.SetValue(Data.Global.RngSeed, result);
            Rng.SetGlobalSeed(result);
            yield return True();
        }
        else
        {
            scope.Throw(SolverError.ExpectedTermOfTypeAt, WellKnown.Types.HexString, arguments[0].Explain());
            yield return False();
            yield break;
        }
    }
}
