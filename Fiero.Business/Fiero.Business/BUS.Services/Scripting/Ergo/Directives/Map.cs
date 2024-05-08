using Ergo.Interpreter;
using Ergo.Interpreter.Directives;
using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;

namespace Fiero.Business;

[SingletonDependency]
public sealed class Map() : InterpreterDirective("Declares a dungeon map.", new("map"), 2, 100000)
{
    public override bool Execute(ErgoInterpreter interpreter, ref InterpreterScope scope, params ITerm[] args)
    {
        var lib = scope.GetLibrary<FieroLib>(FieroLib.Modules.Fiero);
        if (!args[0].Matches<int>(out var Width))
        {
            scope.Throw(ErgoInterpreter.ErrorType.ExpectedTermOfTypeAt, WellKnown.Types.Integer, args[0].Explain());
            return false;
        }
        if (!args[1].Matches<int>(out var Height))
        {
            scope.Throw(ErgoInterpreter.ErrorType.ExpectedTermOfTypeAt, WellKnown.Types.Integer, args[1].Explain());
            return false;
        }
        if (!lib.DeclareMap(scope.Entry, new(Width, Height)))
        {
            scope.Throw(ErgoInterpreter.ErrorType.ModuleNameClash, scope.Entry.Explain());
            return false;
        }
        return true;
    }
}
