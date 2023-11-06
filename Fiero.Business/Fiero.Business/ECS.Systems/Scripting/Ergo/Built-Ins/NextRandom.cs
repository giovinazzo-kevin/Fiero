using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using Ergo.Solver;
using Ergo.Solver.BuiltIns;
using System.Collections.Immutable;

namespace Fiero.Business;

[SingletonDependency]
public sealed class NextRandom : BuiltIn
{
    public NextRandom()
        : base("", new("rng"), 1, ScriptingSystem.RandomModule)
    {
    }

    public override IEnumerable<Evaluation> Apply(SolverContext solver, SolverScope scope, ImmutableArray<ITerm> arguments)
    {
        var N = Rng.Random.NextDouble();
        if (arguments[0].Unify(new Atom(N)).TryGetValue(out var subs))
        {
            yield return True(subs);
        }
        else
        {
            yield return False();
            yield break;
        }
    }
}
