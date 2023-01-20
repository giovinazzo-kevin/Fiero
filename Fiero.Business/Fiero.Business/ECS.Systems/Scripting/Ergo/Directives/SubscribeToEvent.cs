

using Ergo.Interpreter;
using Ergo.Interpreter.Directives;
using Ergo.Lang.Ast;
using Ergo.Lang.Exceptions;
using Ergo.Lang.Extensions;

namespace Fiero.Business;
public class SubscribeToEvent : InterpreterDirective
{
    public SubscribeToEvent()
        : base("", new("subscribe"), 1, 200)
    {
    }

    public override bool Execute(ErgoInterpreter interpreter, ref InterpreterScope scope, params ITerm[] args)
    {
        var lib = scope.GetLibrary<FieroLib>(ErgoScriptingSystem.FieroModule);
        if (!args[0].IsAbstract<List>().TryGetValue(out var list))
        {
            throw new InterpreterException(InterpreterError.ExpectedTermOfTypeAt, scope, WellKnown.Types.List, args[0].Explain());
        }
        lib.SubscribeScriptToEvents(scope.Entry, list);
        return true;
    }
}