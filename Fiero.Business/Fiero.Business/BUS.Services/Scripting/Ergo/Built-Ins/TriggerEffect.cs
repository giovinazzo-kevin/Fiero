using Ergo.Lang;
using Ergo.Lang.Extensions;
using Ergo.Runtime;
using Ergo.Runtime.BuiltIns;
using LightInject;

namespace Fiero.Business;

[SingletonDependency]
public sealed class TriggerEffect(IServiceFactory services) : BuiltIn("", new("effect"), 3, FieroLib.Modules.Effect)
{
    [Term(Functor = "effect_def", Marshalling = TermMarshalling.Named)]
    internal readonly record struct EffectDefStub(EffectName Name, string Arguments, string Duration, string Chance, string CanStack);

    private readonly IServiceFactory _services = services;
    public override ErgoVM.Op Compile()
    {
        var gameEntities = _services.GetInstance<GameEntities>();
        var systems = _services.GetInstance<MetaSystem>();
        return vm =>
        {
            var args = vm.Args;
            if (args[0].Match<EffectDefStub>(out var stub))
            {
                int? duration = int.TryParse(stub.Duration, out var d) ? d : null;
                float? chance = float.TryParse(stub.Chance, out var c) ? c : null;
                bool canStack = bool.TryParse(stub.CanStack, out var b) ? b : false;

                if (args[1].IsEntity<Entity>().TryGetValue(out var e))
                {
                    var def = new EffectDef(stub.Name, stub.Arguments, chance: chance, duration: duration, canStack: canStack, source: e);
                    var effect = def.Resolve(null);
                    // TODO: bind effect.end as callable to args[2]
                    effect.Start(systems, e, null);
                }
                else if (args[1].Match(out Location loc)
                    && systems.Get<DungeonSystem>().TryGetTileAt(loc.FloorId, loc.Position, out var tile))
                {
                    var def = new EffectDef(stub.Name, stub.Arguments, chance: chance, duration: duration, canStack: canStack, source: tile);
                    var effect = def.Resolve(null);
                    // TODO: bind effect.end as callable to args[2]
                    effect.Start(systems, tile, null);
                }
                else
                {
                    vm.Fail();
                }
            }
        };
    }
}
