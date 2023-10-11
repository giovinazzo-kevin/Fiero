

using Ergo.Interpreter;
using Ergo.Interpreter.Directives;
using Ergo.Lang.Ast;
using Ergo.Lang.Exceptions;
using Ergo.Lang.Extensions;

namespace Fiero.Business;

[SingletonDependency]
public class SubscribeToEvent : InterpreterDirective
{
    public SubscribeToEvent()
        : base("", new("subscribe"), 2, 200)
    {
    }

    public override bool Execute(ErgoInterpreter interpreter, ref InterpreterScope scope, params ITerm[] args)
    {
        var lib = scope.GetLibrary<FieroLib>(ScriptingSystem.FieroModule);
        if (args[0] is not Atom module)
        {
            throw new InterpreterException(InterpreterError.ExpectedTermOfTypeAt, scope, WellKnown.Types.Atom, args[0].Explain());
        }
        if (!args[1].IsAbstract<List>().TryGetValue(out var list))
        {
            throw new InterpreterException(InterpreterError.ExpectedTermOfTypeAt, scope, WellKnown.Types.List, args[1].Explain());
        }
        foreach (var item in list.Contents)
        {
            if (item is not Atom atom)
            {
                throw new InterpreterException(InterpreterError.ExpectedTermOfTypeAt, scope, WellKnown.Types.Atom, item.Explain());
            }
            lib.SubscribeScriptToEvent(scope.Entry, module, atom);
        }
        return true;
    }
}