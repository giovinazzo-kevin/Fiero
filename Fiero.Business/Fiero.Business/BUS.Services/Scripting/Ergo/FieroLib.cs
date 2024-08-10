using Ergo.Interpreter.Directives;
using Ergo.Interpreter.Libraries;
using Ergo.Lang.Ast;
using Ergo.Runtime.BuiltIns;
using LightInject;

namespace Fiero.Business;

[TransientDependency]
public partial class FieroLib(IServiceFactory services) : Library
{
    public readonly record struct MapDef(Atom Name, Map.MapInfo Info);

    public override Atom Module => Modules.Fiero;

    public readonly Dictionary<Atom, MapDef> Maps = [];

    public bool DeclareMap(Atom name, Map.MapInfo info)
    {
        if (Maps.ContainsKey(name))
            return false;
        Maps[name] = new(name, info);
        return true;
    }


    public override IEnumerable<InterpreterDirective> GetExportedDirectives()
    {
        yield return services.GetInstance<Map>();
    }
    public override IEnumerable<BuiltIn> GetExportedBuiltins()
    {
        yield return services.GetInstance<AnimationRepeatCount>();
        yield return services.GetInstance<AnimationStop>();
        yield return services.GetInstance<At>();
        yield return services.GetInstance<CastEntity>();
        yield return services.GetInstance<ComponentSetValue>();
        yield return services.GetInstance<Database>();
        yield return services.GetInstance<Despawn>();
        yield return services.GetInstance<MsgBox>();
        yield return services.GetInstance<Shape>();
        yield return services.GetInstance<Spawn>();
        yield return services.GetInstance<TriggerAnimation>();
        yield return services.GetInstance<TriggerAnimationBlocking>();
        yield return services.GetInstance<TriggerEffect>();
        yield return services.GetInstance<TriggerSound>();
        yield return services.GetInstance<MakeDialogueTrigger>();
        yield return services.GetInstance<CenterViewOn>();
    }
}
