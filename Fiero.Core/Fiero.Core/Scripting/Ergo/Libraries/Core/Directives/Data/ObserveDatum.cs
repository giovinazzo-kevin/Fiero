

using Ergo.Interpreter;
using Ergo.Interpreter.Directives;
using Ergo.Lang.Ast;
using Ergo.Lang.Exceptions;
using Ergo.Lang.Extensions;

namespace Fiero.Core.Ergo.Libraries.Core.Data;

[SingletonDependency]
public class ObserveDatum : InterpreterDirective
{
    public ObserveDatum()
        : base("", new("observe"), 2, 210)
    {
    }

    public override bool Execute(ErgoInterpreter interpreter, ref InterpreterScope scope, params ITerm[] args)
    {
        var lib = scope.GetLibrary<CoreLib>(CoreErgoModules.Core);
        if (args[0] is not Atom name)
        {
            scope.Throw(ErgoInterpreter.ErrorType.ExpectedTermOfTypeAt, scope, WellKnown.Types.String, args[0].Explain());
            return false;
        }
        if (!args[1].IsAbstract<List>().TryGetValue(out var list))
        {
            scope.Throw(ErgoInterpreter.ErrorType.ExpectedTermOfTypeAt, scope, WellKnown.Types.List, args[1].Explain());
            return false;
        }
        foreach (var item in list.Contents)
        {
            if (item is not Atom atom)
            {
                throw new InterpreterException(ErgoInterpreter.ErrorType.ExpectedTermOfTypeAt, scope, WellKnown.Types.Atom, item.Explain());
            }
            lib.ObserveDatum(scope.Entry, name, atom);
        }
        return true;
    }
}