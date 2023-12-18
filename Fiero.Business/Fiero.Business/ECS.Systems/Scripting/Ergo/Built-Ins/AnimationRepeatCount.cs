using Ergo.Lang.Ast;
using Ergo.Lang.Exceptions;
using Ergo.Lang.Extensions;
using Ergo.Runtime;
using Ergo.Runtime.BuiltIns;
using LightInject;
using System.Collections.Immutable;

namespace Fiero.Business;

[SingletonDependency]
public sealed class AnimationRepeatCount : BuiltIn
{
    protected readonly IServiceFactory Services;
    public AnimationRepeatCount(IServiceFactory services)
        // repeat(id, times).
        : base("", new("repeat"), 2, ScriptingSystem.AnimationModule)
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
        if (!args[1].Matches(out int times))
        {
            yield return ThrowFalse(scope, SolverError.ExpectedTermOfTypeAt, WellKnown.Types.Integer, args[1]);
            yield break;
        }
        var render = Services.GetInstance<RenderSystem>();
        render.AlterAnimation(id, a => a.RepeatCount = times);
        yield return True();
    }
}
