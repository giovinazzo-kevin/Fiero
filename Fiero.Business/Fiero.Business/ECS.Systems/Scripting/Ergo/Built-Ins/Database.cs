using Ergo.Lang.Ast;
using Ergo.Lang.Exceptions;
using Ergo.Lang.Extensions;
using Ergo.Solver;
using Ergo.Solver.BuiltIns;
using System.Collections.Concurrent;
using System.Collections.Immutable;

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

    public override IEnumerable<Evaluation> Apply(SolverContext solver, SolverScope scope, ImmutableArray<ITerm> args)
    {
        if (!args[1].Matches<AccessMode>(out var mode))
        {
            yield return ThrowFalse(scope, SolverError.ExpectedTermOfTypeAt, nameof(AccessMode), args[1]);
            yield break;
        }
        if (!args[0].IsGround && mode != AccessMode.Del)
        {
            yield return ThrowFalse(scope, SolverError.TermNotSufficientlyInstantiated, args[0].Explain());
            yield break;
        }
        switch (mode)
        {
            case AccessMode.Get when Store.TryGetValue(args[0], out var v):
                if (v.Unify(args[2]).TryGetValue(out var subs))
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
                if (args[0].IsGround)
                {
                    Store.TryRemove(args[0], out var d);
                    if (args[2].Unify(d).TryGetValue(out subs))
                    {
                        yield return True(subs);
                        yield break;
                    }
                }
                else
                {
                    foreach (var key in Store.Keys
                        .Where(key => args[0].Unify(key).TryGetValue(out _)))
                    {
                        Store.TryRemove(key, out var d);
                        if (args[2].Unify(d).TryGetValue(out subs))
                        {
                            yield return True(subs);
                            yield break;
                        }
                    }
                }
                break;
        }
        yield return False();
        yield break;
    }
}