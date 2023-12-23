using Ergo.Lang.Ast;
using Ergo.Runtime;
using Ergo.Runtime.BuiltIns;

namespace Fiero.Business
{
    internal partial class ScriptEffectLib
    {
        internal class Subscribed(ScriptEffectLib lib)
            : BuiltIn(string.Empty, new Atom("subscribed"), 2, lib.Module)
        {
            private readonly List<Signature> _subs = lib.CurrentContext.Source.Script.ScriptProperties.SubscribedEvents;

            public override ErgoVM.Op Compile()
            {
                int i = 0;
                return NextSub;
                void NextSub(ErgoVM vm)
                {
                    var sign = _subs[i++];
                    if (i < _subs.Count)
                        vm.PushChoice(NextSub);
                    else i = 0;
                    var module = sign.Module.GetOr(default);
                    var a1 = vm.Arg(1);
                    vm.SetArg(1, module);
                    ErgoVM.Goals.Unify2(vm);
                    if (vm.State == ErgoVM.VMState.Fail)
                        return;
                    vm.SetArg(0, a1);
                    vm.SetArg(1, sign.Functor);
                    ErgoVM.Goals.Unify2(vm);
                }
            }
        }
    }
}
