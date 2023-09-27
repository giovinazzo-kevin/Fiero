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
        // effect(effect_def(poison, 1), Owner, EndEffect),
        // call(EndEffect).
        : base("", new("effect"), 3, ErgoScriptingSystem.FieroModule)
    {
        _services = services;
    }

    public override IEnumerable<Evaluation> Apply(SolverContext solver, SolverScope scope, ITerm[] arguments)
    {
        var gameEntities = _services.GetInstance<GameEntities>();
        if (arguments[0].Matches<EffectDefStub>(out var stub)
        && arguments[1].Matches<Entity>(out var e)
        && gameEntities.TryGetProxy(e.Id, out e))
        {
            var def = new EffectDef(stub.Name, stub.Arguments, source: e);
            var effect = def.Resolve(null);
            effect.Start(_services.GetInstance<GameSystems>(), e);
        }
        yield return True();
    }
}
