using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using Ergo.Runtime;
using Ergo.Runtime.BuiltIns;

namespace Fiero.Core;

[SingletonDependency]
public class Get(GameDataStore store)
    : BuiltIn("Gets the value of a game datum", new Atom("get"), 2, ErgoModules.Data)
{
    public override ErgoVM.Op Compile() => vm =>
    {
        if (!vm.Arg(0).Matches(out string name))
        {
            vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, typeof(GameDatum), vm.Arg(0).Explain());
            return;
        }
        var datum = store.GetRegisteredDatumType(name.ToCSharpCase());
        var val = store.Get(datum);
        var term = TermMarshall.ToTerm(val, datum.T);
        vm.SetArg(0, term);
        ErgoVM.Goals.Unify2(vm);
    };
}
