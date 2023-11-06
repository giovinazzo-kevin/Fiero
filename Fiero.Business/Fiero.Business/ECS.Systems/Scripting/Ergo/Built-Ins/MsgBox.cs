using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using Ergo.Solver;
using Ergo.Solver.BuiltIns;
using System.Collections.Immutable;

namespace Fiero.Business;

[SingletonDependency]
public sealed class MsgBox : BuiltIn
{
    public readonly GameUI UI;

    public MsgBox(GameUI ui)
        : base("", new("msg_box"), 3, ScriptingSystem.FieroModule)
    {
        UI = ui;
    }

    public override IEnumerable<Evaluation> Apply(SolverContext solver, SolverScope scope, ImmutableArray<ITerm> arguments)
    {
        var modal = UI.NecessaryChoice(Array.Empty<string>(), arguments[0].AsQuoted(false).Explain(), arguments[1].AsQuoted(false).Explain());
        // Block this thread until the user closes this modal
        var choice = modal.WaitForClose().GetAwaiter().GetResult();
        var term = TermMarshall.ToTerm(choice);
        if (arguments[2].Unify(term).TryGetValue(out var subs))
        {
            yield return True(subs);
            yield break;
        }
        yield return False();
    }
}
