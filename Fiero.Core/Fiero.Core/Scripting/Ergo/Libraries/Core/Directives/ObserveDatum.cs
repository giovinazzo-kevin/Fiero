

using Ergo.Interpreter;
using Ergo.Interpreter.Directives;
using Ergo.Lang.Ast;
using Ergo.Lang.Exceptions;
using Ergo.Lang.Extensions;

namespace Fiero.Core;

[SingletonDependency]
public class ObserveDatum : InterpreterDirective
{
    public ObserveDatum()
        : base("", new("observe"), 1, 210)
    {
    }

    public override bool Execute(ErgoInterpreter interpreter, ref InterpreterScope scope, params ITerm[] args)
    {
        var lib = scope.GetLibrary<CoreLib>(ErgoModules.Core);
        if (!args[0].IsAbstract<List>().TryGetValue(out var list))
        {
            throw new InterpreterException(ErgoInterpreter.ErrorType.ExpectedTermOfTypeAt, scope, WellKnown.Types.List, args[0].Explain());
        }
        foreach (var item in list.Contents)
        {
            if (item is not Atom atom)
            {
                throw new InterpreterException(ErgoInterpreter.ErrorType.ExpectedTermOfTypeAt, scope, WellKnown.Types.Atom, item.Explain());
            }
            lib.ObserveDatum(scope.Entry, atom.Explain());
        }
        return true;
    }
}