using Ergo.Lang.Ast;
using Ergo.Solver;
using Ergo.Solver.BuiltIns;

namespace Fiero.Business;

[SingletonDependency]
public sealed class MsgBox : SolverBuiltIn
{
    public readonly GameUI UI;

    public MsgBox(GameUI ui)
        : base("", new("msg_box"), 3, ErgoScriptingSystem.FieroModule)
    {
        UI = ui;
    }

    public override IEnumerable<Evaluation> Apply(SolverContext solver, SolverScope scope, ITerm[] arguments)
    {
        UI.OptionalChoice(new[] { arguments[0].Explain() }, arguments[1].Explain());
        yield return True();
    }
}
