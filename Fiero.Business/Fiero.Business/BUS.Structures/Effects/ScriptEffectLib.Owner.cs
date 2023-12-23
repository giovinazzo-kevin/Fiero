using Ergo.Lang.Ast;
using Ergo.Runtime;
using Ergo.Runtime.BuiltIns;

namespace Fiero.Business
{
    internal partial class ScriptEffectLib
    {
        internal class Owner(ScriptEffectLib lib)
            : BuiltIn(string.Empty, new Atom("owner_"), 1, lib.Module)
        {
            public override ErgoVM.Op Compile()
            {
                return vm =>
                {
                    vm.SetArg(1, new EntityAsTerm(lib.CurrentOwner, lib.CurrentContext.Owner.ErgoType()));
                    ErgoVM.Goals.Unify2(vm);
                };
            }
        }
    }
}
