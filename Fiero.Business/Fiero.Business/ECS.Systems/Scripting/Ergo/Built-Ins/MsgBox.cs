using Ergo.Lang.Ast;
using Ergo.Solver;
using Ergo.Solver.BuiltIns;

namespace Fiero.Business;

[SingletonDependency]
public sealed class MsgBox : SolverBuiltIn
{
    public readonly GameUI UI;

    public MsgBox(GameUI ui)
        : base("", new("msg_box"), 2, ErgoScriptingSystem.FieroModule)
    {
        UI = ui;
    }

    public override IEnumerable<Evaluation> Apply(SolverContext solver, SolverScope scope, ITerm[] arguments)
    {
        var choice = UI.NecessaryChoice(Array.Empty<string>(), arguments[0].AsQuoted(false).Explain(), arguments[1].AsQuoted(false).Explain());
        yield return True();
    }
}
