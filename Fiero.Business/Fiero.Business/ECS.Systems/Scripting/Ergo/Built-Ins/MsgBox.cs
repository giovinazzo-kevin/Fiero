using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using Ergo.Solver;
using Ergo.Solver.BuiltIns;

namespace Fiero.Business;

[SingletonDependency]
public sealed class MsgBox : SolverBuiltIn
{
    public readonly GameUI UI;

    public MsgBox(GameUI ui)
        : base("", new("msg_box"), 3, ScriptingSystem.FieroModule)
    {
        UI = ui;
    }

    public override IEnumerable<Evaluation> Apply(SolverContext solver, SolverScope scope, ITerm[] arguments)
    {
        var modal = UI.NecessaryChoice(Array.Empty<string>(), arguments[0].AsQuoted(false).Explain(), arguments[1].AsQuoted(false).Explain());
        // Block this thread until the user closes this modal
        ModalWindowButton choice;
        if (!GameThread.IsMainThread.Value)
        {
            choice = modal.WaitForClose().GetAwaiter().GetResult();
        }
        else
        {

        }
        var term = TermMarshall.ToTerm(choice);
        if (arguments[2].Unify(term).TryGetValue(out var subs))
        {
            yield return True(subs);
            yield break;
        }
        yield return False();
    }
}
