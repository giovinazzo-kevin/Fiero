using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using Ergo.Runtime;
using Ergo.Runtime.BuiltIns;

namespace Fiero.Core;

[SingletonDependency]
public class Set(GameDataStore store)
    : BuiltIn("Sets the value of a game datum if its current value matches the comparison value", new Atom("set"), 3, ErgoModules.Data)
{
    public override ErgoVM.Op Compile() => vm =>
    {
        if (!vm.Arg(0).Matches(out string name))
        {
            vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, typeof(GameDatum), vm.Arg(0).Explain());
            return;
        }
        var datum = store.GetRegisteredDatumType(name.ToCSharpCase());
        if (!vm.Arg(2).MatchesUntyped(out var obj, datum.T))
        {
            vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, datum.T.Name, vm.Arg(2).Explain());
            return;
        }
        var val = store.Get(datum);
        var term = TermMarshall.ToTerm(val, datum.T);
        // Unify the current value and the second argument (which can be unbound for an unconditional set).
        vm.SetArg(0, term);
        ErgoVM.Goals.Unify2(vm);
        if (vm.State == ErgoVM.VMState.Fail)
            return;
        // Set the value if the unification succeeds
        store.SetValue(datum, obj);
    };
}