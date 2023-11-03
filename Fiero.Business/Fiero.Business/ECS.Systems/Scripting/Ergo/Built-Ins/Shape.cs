using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Exceptions;
using Ergo.Lang.Extensions;
using Ergo.Solver;
using Ergo.Solver.BuiltIns;
using LightInject;
using System.Collections.Immutable;

namespace Fiero.Business;

[SingletonDependency]
public sealed class Shape : SolverBuiltIn
{
    public readonly IServiceFactory Services;

    public Shape(IServiceFactory services)
        : base("", new("shape"), 4, ScriptingSystem.FieroModule)
    {
        Services = services;
    }

    public override IEnumerable<Evaluation> Apply(SolverContext solver, SolverScope scope, ImmutableArray<ITerm> args)
    {
        if (!args[0].Matches<ShapeName>(out var shape))
        {
            yield return ThrowFalse(scope, SolverError.ExpectedTermOfTypeAt, nameof(ShapeName), args[0]);
            yield break;
        }
        if (!args[1].Matches<Coord>(out var center))
        {
            yield return ThrowFalse(scope, SolverError.ExpectedTermOfTypeAt, nameof(Coord), args[1]);
            yield break;
        }
        var enumerable = Enumerable.Empty<Coord>();
        if (args[2].Matches<int>(out var iSize))
        {
            enumerable = shape switch
            {
                ShapeName.Box => Shapes.Box(Coord.Zero, iSize),
                ShapeName.Square => Shapes.Neighborhood(Coord.Zero, iSize),
                ShapeName.SquareSpiral => Shapes.SquareSpiral(Coord.Zero, iSize),
                ShapeName.Disc => Shapes.Disc(Coord.Zero, iSize),
                ShapeName.Circle => Shapes.Circle(Coord.Zero, iSize),
                _ => enumerable
            };
        }
        else if (args[2].Matches<Coord>(out var pSize))
        {
            enumerable = shape switch
            {
                ShapeName.Rect => Shapes.Rect(Coord.Zero, pSize),
                ShapeName.Line => Shapes.Line(Coord.Zero, pSize),
                _ => enumerable
            };
        }
        else
        {
            yield return ThrowFalse(scope, SolverError.ExpectedTermOfTypeAt, WellKnown.Types.Integer, args[1]);
            yield break;
        }
        if (!enumerable.Any())
        {
            yield return False();
            yield break;
        }
        var any = false;
        foreach (var p in enumerable
            .Select(x => x + center))
        {
            var term = TermMarshall.ToTerm(p, functor: new Atom(nameof(p)), mode: TermMarshalling.Positional);
            if (args[3].Unify(term).TryGetValue(out var subs))
            {
                yield return True(subs);
                any = true;
            }
        }
        if (!any)
            yield return False();
    }
}
