using Ergo.Lang;
using Ergo.Lang.Extensions;
using Ergo.Runtime;
using Ergo.Runtime.BuiltIns;
using LightInject;

namespace Fiero.Business;

[SingletonDependency]
public sealed class TriggerEffect(IServiceFactory services) : BuiltIn("", new("effect"), 3, ScriptingSystem.EffectModule)
{
    [Term(Functor = "effect_def", Marshalling = TermMarshalling.Positional)]
    internal readonly record struct EffectDefStub(EffectName Name, string Arguments);

    private readonly IServiceFactory _services = services;
    public override ErgoVM.Op Compile()
    {
        var gameEntities = _services.GetInstance<GameEntities>();
        var systems = _services.GetInstance<GameSystems>();
        return vm =>
        {
            var args = vm.Args;
            if (args[0].Matches<EffectDefStub>(out var stub))
            {
                if (args[1].IsEntity<Entity>().TryGetValue(out var e))
                {
                    var def = new EffectDef(stub.Name, stub.Arguments, source: e);
                    var effect = def.Resolve(null);
                    // TODO: bind effect.end as callable to args[2]
                    effect.Start(systems, e);
                }
                else if (args[1].Matches(out Location loc)
                    && systems.Dungeon.TryGetTileAt(loc.FloorId, loc.Position, out var tile))
                {
                    var def = new EffectDef(stub.Name, stub.Arguments, source: tile);
                    var effect = def.Resolve(null);
                    // TODO: bind effect.end as callable to args[2]
                    effect.Start(systems, tile);
                }
                else
                {
                    vm.Fail();
                }
            }
        };
    }
}
