using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using Ergo.Runtime;
using Ergo.Runtime.BuiltIns;

namespace Fiero.Business;

[SingletonDependency]
public sealed class Database : BuiltIn
{
    public enum AccessMode
    {
        Get,
        Set,
        Del
    }

    private readonly Dictionary<KnowledgeBase, Dictionary<ITerm, ITerm>> Store = new();

    public Database()
        : base("", new("db"), 3, FieroLib.Modules.Data)
    {
    }

    public override ErgoVM.Op Compile()
    {
        return vm =>
        {
            if (!Store.TryGetValue(vm.KB, out var store))
                Store[vm.KB] = store = new();
            var args = vm.Args;
            if (!args[1].Match<AccessMode>(out var mode))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, nameof(AccessMode), args[1]);
                return;
            }
            if (!args[0].IsGround && mode != AccessMode.Del)
            {
                vm.Throw(ErgoVM.ErrorType.TermNotSufficientlyInstantiated, args[0].Explain());
                return;
            }
            switch (mode)
            {
                default:
                    vm.Fail();
                    break;
                case AccessMode.Get when store.TryGetValue(args[0], out var v):
                    vm.SetArg(0, v);
                    vm.SetArg(1, args[2]);
                    ErgoVM.Goals.Unify2(vm);
                    break;
                case AccessMode.Set:
                    store[args[0]] = args[2];
                    break;
                case AccessMode.Del:
                    if (args[0].IsGround)
                    {
                        store.Remove(args[0], out var d);
                        vm.SetArg(0, args[2]);
                        vm.SetArg(1, d);
                        ErgoVM.Goals.Unify2(vm);
                    }
                    else
                    {
                        int i = 0;
                        var a2 = args[2];
                        DeleteNextKey(vm);
                        void DeleteNextKey(ErgoVM vm)
                        {
                            var key = store.Keys.ElementAt(i++);
                            if (i < store.Keys.Count)
                                vm.PushChoice(DeleteNextKey);
                            vm.SetArg(1, key);
                            ErgoVM.Goals.Unify2(vm);
                            if (vm.State == ErgoVM.VMState.Fail)
                                return;
                            store.Remove(key, out var d);
                            vm.SetArg(0, a2);
                            vm.SetArg(1, d);
                            ErgoVM.Goals.Unify2(vm);
                        }
                    }
                    break;
            }
        };
    }
}