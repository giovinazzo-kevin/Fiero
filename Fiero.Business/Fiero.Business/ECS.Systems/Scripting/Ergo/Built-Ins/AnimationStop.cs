using Ergo.Lang.Ast;
using Ergo.Lang.Exceptions;
using Ergo.Lang.Extensions;
using Ergo.Solver;
using Ergo.Solver.BuiltIns;
using LightInject;
using System.Collections.Immutable;

namespace Fiero.Business;

[SingletonDependency]
public sealed class AnimationStop : SolverBuiltIn
{
    protected readonly IServiceFactory Services;
    public AnimationStop(IServiceFactory services)
        // repeat(id, times).
        : base("", new("stop_animation"), 1, ScriptingSystem.AnimationModule)
    {
        Services = services;
    }

    public override IEnumerable<Evaluation> Apply(SolverContext solver, SolverScope scope, ImmutableArray<ITerm> args)
    {
        if (!args[0].Matches(out int id))
        {
            yield return ThrowFalse(scope, SolverError.ExpectedTermOfTypeAt, WellKnown.Types.Integer, args[0]);
            yield break;
        }
        var render = Services.GetInstance<RenderSystem>();
        render.StopAnimation(id);
        yield return True();
    }
}
