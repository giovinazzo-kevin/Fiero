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
        : base("", new("effect"), 3, ScriptingSystem.EffectModule)
    {
        _services = services;
    }

    public override IEnumerable<Evaluation> Apply(SolverContext solver, SolverScope scope, ITerm[] arguments)
    {
        var gameEntities = _services.GetInstance<GameEntities>();
        var floorSys = _services.GetInstance<DungeonSystem>();
        if (arguments[0].Matches<EffectDefStub>(out var stub))
        {
            if (arguments[1].IsEntity<Entity>().TryGetValue(out var e))
            {
                var def = new EffectDef(stub.Name, stub.Arguments, source: e);
                var effect = def.Resolve(null);
                // TODO: bind effect.end as callable to args[2]
                effect.Start(_services.GetInstance<GameSystems>(), e);
            }
            else if (arguments[1].Matches(out Location loc)
                && floorSys.TryGetTileAt(loc.FloorId, loc.Position, out var tile))
            {
                var def = new EffectDef(stub.Name, stub.Arguments, source: tile);
                var effect = def.Resolve(null);
                // TODO: bind effect.end as callable to args[2]
                effect.Start(_services.GetInstance<GameSystems>(), tile);
            }
            else
            {
                yield return False();
                yield break;
            }
        }
        yield return True();
    }
}
