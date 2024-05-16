using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using Ergo.Runtime;
using Ergo.Runtime.BuiltIns;

namespace Fiero.Core.Ergo.Libraries.Core.Data;

[SingletonDependency]
public class GetDatum(GameDataStore store)
    : BuiltIn("Gets the value of a game datum", new Atom("get"), 3, CoreErgoModules.Data)
{
    public override ErgoVM.Op Compile() => vm =>
    {
        if (!vm.Arg(0).Matches(out string module))
        {
            vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, typeof(string), vm.Arg(0).Explain());
            return;
        }
        if (!vm.Arg(1).Matches(out string name))
        {
            vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, typeof(string), vm.Arg(1).Explain());
            return;
        }
        var (m, n) = (module.ToCSharpCase(), name.ToCSharpCase());
        if (!store.TryGetRegisteredDatumType(m, n, out var datum))
            store.Register((ErgoDatum)(datum = new ErgoDatum(m, n)), WellKnown.Literals.Discard);
        var val = store.Get(datum);
        var term = TermMarshall.ToTerm(val, datum.T);
        vm.SetArg(0, term);
        vm.SetArg(1, vm.Arg(2));
        ErgoVM.Goals.Unify2(vm);
    };
}
