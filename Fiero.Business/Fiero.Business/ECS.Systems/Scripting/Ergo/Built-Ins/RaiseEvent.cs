using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Exceptions;
using Ergo.Lang.Extensions;
using Ergo.Solver;
using Ergo.Solver.BuiltIns;
using LightInject;
using System.Reflection;

namespace Fiero.Business;

[SingletonDependency]
public sealed class RaiseEvent : SolverBuiltIn
{
    private IServiceFactory _services;
    public RaiseEvent(IServiceFactory services)
        : base("", new("raise"), 3, ErgoScriptingSystem.FieroModule)
    {
        _services = services;
    }

    public override IEnumerable<Evaluation> Apply(SolverContext solver, SolverScope scope, ITerm[] arguments)
    {
        if (!arguments[0].Matches(out string sysName))
        {
            yield return ThrowFalse(scope, SolverError.ExpectedTermOfTypeAt, WellKnown.Types.String, arguments[0]);
            yield break;
        }
        if (!arguments[1].Matches(out string eventname))
        {
            yield return ThrowFalse(scope, SolverError.ExpectedTermOfTypeAt, WellKnown.Types.String, arguments[1]);
            yield break;
        }
        var any = false;
        var gameSystems = _services.GetInstance<GameSystems>();
        foreach (var field in MetaSystem.GetSystemEventFields())
        {
            if (field.System.Name.ToErgoCase().Replace("System", string.Empty, StringComparison.OrdinalIgnoreCase) != sysName)
                continue;
            if (field.Field.Name.ToErgoCase().Replace("Event", string.Empty, StringComparison.OrdinalIgnoreCase) != eventname)
                continue;
            if (!arguments[2].IsAbstract<Dict>().TryGetValue(out var dict))
            {
                yield return ThrowFalse(scope, SolverError.ExpectedTermOfTypeAt, WellKnown.Types.Dictionary, arguments[2]);
                yield break;
            }
            var b = field.Field.FieldType.BaseType;
            if (!b.IsGenericType || !b.GetGenericTypeDefinition().IsAssignableFrom(typeof(SystemEvent<,>)))
            {
                yield return ThrowFalse(scope, SolverError.ExpectedTermOfTypeAt, "SystemEvent", field);
                yield break;
            }
            var tArgs = field.Field.FieldType.BaseType.GetGenericArguments()[1];
            //if (tArgs.Name.ToErgoCase() != functor.Explain())
            //    continue;
            var obj = field.Field.GetValue(field.System.GetValue(gameSystems));
            var arg = TermMarshall.FromTerm(arguments[2], tArgs, TermMarshalling.Named);
            field.Field.FieldType.GetMethod("Raise", BindingFlags.Public | BindingFlags.Instance)
                .Invoke(obj, new[] { arg });
            any = true;
        }
        if (any)
        {
            yield return True();
            yield break;
        }
        yield return False();
        yield break;
    }
}