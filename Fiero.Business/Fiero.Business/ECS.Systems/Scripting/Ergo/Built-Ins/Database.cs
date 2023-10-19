using Ergo.Lang.Ast;
using Ergo.Lang.Exceptions;
using Ergo.Lang.Extensions;
using Ergo.Solver;
using Ergo.Solver.BuiltIns;
using System.Collections.Concurrent;

namespace Fiero.Business;

[SingletonDependency]
public sealed class Database : SolverBuiltIn
{
    public enum AccessMode
    {
        Get,
        Set,
        Del
    }

    private readonly ConcurrentDictionary<ITerm, ITerm> Store = new();

    public Database()
        : base("", new("db"), 3, ScriptingSystem.DataModule)
    {
    }

    public override IEnumerable<Evaluation> Apply(SolverContext solver, SolverScope scope, ITerm[] args)
    {
        if (!args[0].IsGround)
        {
            yield return ThrowFalse(scope, SolverError.TermNotSufficientlyInstantiated, args[0].Explain());
            yield break;
        }
        if (!args[1].Matches<AccessMode>(out var mode))
        {
            yield return ThrowFalse(scope, SolverError.ExpectedTermOfTypeAt, nameof(AccessMode), args[1]);
            yield break;
        }
        switch (mode)
        {
            case AccessMode.Get when Store.TryGetValue(args[0], out var v):
                if (args[2].Unify(v).TryGetValue(out var subs))
                {
                    yield return True(subs);
                    yield break;
                }
                break;
            case AccessMode.Set:
                Store[args[0]] = args[2];
                yield return True();
                yield break;
            case AccessMode.Del:
                Store.TryRemove(args[0], out var d);
                if (args[2].Unify(d).TryGetValue(out subs))
                {
                    yield return True(subs);
                    yield break;
                }
                break;
        }
        yield return False();
        yield break;
    }
}