using Ergo.Lang.Ast;
using Ergo.Runtime;
using Ergo.Runtime.BuiltIns;

namespace Fiero.Business
{
    internal partial class ScriptEffectLib
    {
        internal class End(ScriptEffectLib lib)
            : BuiltIn(string.Empty, new Atom("end"), 0, lib.Module)
        {
            private bool _ended = false;
            public override ErgoVM.Op Compile()
            {
                return vm =>
                {
                    if (_ended)
                        vm.Fail();
                    else
                    {
                        lib.CurrentContext.Source.End(lib.Systems, lib.CurrentContext.Owner);
                        _ended = true;
                    }
                };
            }
        }
    }
}
