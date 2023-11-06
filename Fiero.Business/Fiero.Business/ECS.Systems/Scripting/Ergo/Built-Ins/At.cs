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
public sealed class At : BuiltIn
{
    public readonly IServiceFactory Services;

    public At(IServiceFactory services)
        : base("", new("at"), 2, ScriptingSystem.FieroModule)
    {
        Services = services;
    }

    public override IEnumerable<Evaluation> Apply(SolverContext solver, SolverScope scope, ImmutableArray<ITerm> args)
    {
        Location loc;
        if (args[0].IsEntity<PhysicalEntity>().TryGetValue(out var entity))
        {
            loc = entity.Location();
        }
        else if (!args[0].Matches(out loc))
        {
            yield return ThrowFalse(scope, SolverError.ExpectedTermOfTypeAt, nameof(Location), args[0]);
            yield break;
        }
        var systems = Services.GetInstance<GameSystems>();
        var cell = systems.Dungeon.GetCellAt(loc.FloorId, loc.Position);
        if (cell is null)
        {
            yield return False();
            yield break;
        }
        var any = false;
        var term = TermMarshall.ToTerm(cell.Tile);
        if (args[2].Unify(term).TryGetValue(out var subs))
        {
            yield return True(subs);
            any = true;
        }
        foreach (var A in cell.Actors)
        {
            term = new EntityAsTerm(A.Id, A.ErgoType());
            if (args[2].Unify(term).TryGetValue(out subs))
            {
                yield return True(subs);
                any = true;
            }
        }
        foreach (var F in cell.Features)
        {
            term = new EntityAsTerm(F.Id, F.ErgoType());
            if (args[2].Unify(term).TryGetValue(out subs))
            {
                yield return True(subs);
                any = true;
            }
        }
        foreach (var I in cell.Items)
        {
            term = new EntityAsTerm(I.Id, I.ErgoType());
            if (args[2].Unify(term).TryGetValue(out subs))
            {
                yield return True(subs);
                any = true;
            }
        }
        if (!any)
            yield return False();
    }
}
