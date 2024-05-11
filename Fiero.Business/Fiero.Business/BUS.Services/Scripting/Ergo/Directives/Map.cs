using Ergo.Interpreter;
using Ergo.Interpreter.Directives;
using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;

namespace Fiero.Business;

[SingletonDependency]
public sealed class Map() : InterpreterDirective("Declares a dungeon map.", new("map"), 1, 100000)
{
    [Term(Marshalling = TermMarshalling.Named)]
    public readonly record struct MapPools(string[] Monster, string[] Item);
    [Term(Marshalling = TermMarshalling.Named)]
    public readonly record struct MapInfo(Coord Size, MapPools Pools);

    public override bool Execute(ErgoInterpreter interpreter, ref InterpreterScope scope, params ITerm[] args)
    {
        var lib = scope.GetLibrary<FieroLib>(FieroLib.Modules.Fiero);
        if (!args[0].Matches<MapInfo>(out var mapInfo))
        {
            scope.Throw(ErgoInterpreter.ErrorType.ExpectedTermOfTypeAt, nameof(MapInfo), args[0].Explain());
            return false;
        }
        if (!lib.DeclareMap(scope.Entry, mapInfo))
        {
            scope.Throw(ErgoInterpreter.ErrorType.ModuleNameClash, scope.Entry.Explain());
            return false;
        }
        return true;
    }
}
