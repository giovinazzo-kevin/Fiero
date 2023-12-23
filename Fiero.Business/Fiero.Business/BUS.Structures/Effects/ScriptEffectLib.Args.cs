using Ergo.Lang.Ast;
using Ergo.Runtime;
using Ergo.Runtime.BuiltIns;

namespace Fiero.Business
{
    internal partial class ScriptEffectLib
    {
        internal class Args(ScriptEffectLib lib)
            : BuiltIn(string.Empty, new Atom("args"), 1, lib.Module)
        {
            public override ErgoVM.Op Compile()
            {
                return vm =>
                {
                    if (!vm.KB.Scope.Parse<ITerm>(lib.CurrentContext.Args).TryGetValue(out var term))
                    {
                        vm.Fail();
                        return;
                    }
                    vm.SetArg(1, term);
                    ErgoVM.Goals.Unify2(vm);
                };
            }
        }
    }
}
