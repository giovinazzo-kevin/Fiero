using Ergo.Lang;
using Ergo.Runtime;
using Ergo.Runtime.BuiltIns;

namespace Fiero.Business;

[SingletonDependency]
public sealed class MsgBox(GameUI ui) : BuiltIn("", new("msg_box"), 3, FieroLib.Modules.Fiero)
{
    public readonly GameUI UI = ui;

    public override ErgoVM.Op Compile()
    {
        return vm =>
        {
            var args = vm.Args;
            var modal = UI.NecessaryChoice(Array.Empty<string>(), args[0].AsQuoted(false).Explain(), args[1].AsQuoted(false).Explain());
            // Block this thread until the user closes this modal
            var choice = modal.WaitForClose().GetAwaiter().GetResult();
            var term = TermMarshall.ToTerm(choice);
            vm.SetArg(0, args[2]);
            vm.SetArg(1, term);
            ErgoVM.Goals.Unify2(vm);
        };
    }
}
