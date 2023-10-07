using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Exceptions;
using Ergo.Lang.Extensions;
using Ergo.Solver;
using Ergo.Solver.BuiltIns;
using LightInject;

namespace Fiero.Business;
[SingletonDependency]
public sealed class At : SolverBuiltIn
{
    public readonly IServiceFactory Services;

    public At(IServiceFactory services)
        : base("", new("at"), 3, ErgoScriptingSystem.FieroModule)
    {
        Services = services;
    }

    public override IEnumerable<Evaluation> Apply(SolverContext solver, SolverScope scope, ITerm[] args)
    {
        if (!args[0].Matches<FloorId>(out var f))
        {
            yield return ThrowFalse(scope, SolverError.ExpectedTermOfTypeAt, nameof(FloorId), args[0]);
            yield break;
        }
        if (!args[1].Matches<Coord>(out var p))
        {
            yield return ThrowFalse(scope, SolverError.ExpectedTermOfTypeAt, nameof(Coord), args[1]);
            yield break;
        }
        var systems = Services.GetInstance<GameSystems>();
        var cell = systems.Dungeon.GetCellAt(f, p);
        if (cell is null)
        {
            yield return False();
            yield break;
        }
        var term = TermMarshall.ToTerm(cell.Tile);
        if (args[2].Unify(term).TryGetValue(out var subs))
        {
            yield return True(subs);
        }
        foreach (var A in cell.Actors)
        {
            term = TermMarshall.ToTerm(A);
            if (args[2].Unify(term).TryGetValue(out subs))
            {
                yield return True(subs);
            }
        }
        foreach (var F in cell.Features)
        {
            term = TermMarshall.ToTerm(F);
            if (args[2].Unify(term).TryGetValue(out subs))
            {
                yield return True(subs);
            }
        }
        foreach (var I in cell.Items)
        {
            term = TermMarshall.ToTerm(I);
            if (args[2].Unify(term).TryGetValue(out subs))
            {
                yield return True(subs);
            }
        }
    }
}
