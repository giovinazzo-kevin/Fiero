using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using Ergo.Solver;
using Ergo.Solver.BuiltIns;
using LightInject;
using System.Collections.Immutable;

namespace Fiero.Business;

[SingletonDependency]
public sealed class Despawn : BuiltIn
{
    protected readonly IServiceFactory _services;

    public Despawn(IServiceFactory services)
        : base("", new Atom("despawn"), 1, ScriptingSystem.FieroModule)
    {
        _services = services;
    }

    public override IEnumerable<Evaluation> Apply(SolverContext solver, SolverScope scope, ImmutableArray<ITerm> arguments)
    {
        if (arguments[0].Matches<int>(out var id))
        {
            var action = _services.GetInstance<ActionSystem>();
            var entities = _services.GetInstance<GameEntities>();
            if (entities.TryGetProxy<Entity>(id, out var entity))
            {
                action.Despawn(entity);
            }
            yield return True();
            yield break;
        }
        yield return False();
        yield break;
    }
}
