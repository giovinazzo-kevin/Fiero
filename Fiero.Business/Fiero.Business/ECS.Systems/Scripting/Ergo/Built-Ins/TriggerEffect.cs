using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using Ergo.Solver;
using Ergo.Solver.BuiltIns;
using LightInject;

namespace Fiero.Business;

[SingletonDependency]
public sealed class TriggerEffect : SolverBuiltIn
{
    [Term(Functor = "effect_def", Marshalling = TermMarshalling.Positional)]
    internal readonly record struct EffectDefStub(EffectName Name, string Arguments);

    private IServiceFactory _services;

    public TriggerEffect(IServiceFactory services)
        : base("", new("start"), 3, ErgoScriptingSystem.EffectModule)
    {
        _services = services;
    }

    public override IEnumerable<Evaluation> Apply(SolverContext solver, SolverScope scope, ITerm[] arguments)
    {
        var gameEntities = _services.GetInstance<GameEntities>();
        if (arguments[0].Matches<EffectDefStub>(out var stub)
        && gameEntities.TryParseTerm(arguments[1], out var e))
        {
            var def = new EffectDef(stub.Name, stub.Arguments, source: (Entity)e);
            var effect = def.Resolve(null);
            // TODO: bind effect.end as callable to args[2]
            effect.Start(_services.GetInstance<GameSystems>(), (Entity)e);
        }
        yield return True();
    }
}
