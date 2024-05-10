using Ergo.Interpreter.Directives;
using Ergo.Interpreter.Libraries;
using Ergo.Lang.Ast;
using Ergo.Runtime.BuiltIns;
using LightInject;

namespace Fiero.Business;
public partial class FieroLib
{
    public static class Types
    {
        public const string Entity = nameof(Entity);
        public const string EntityType = nameof(EntityType);
        public const string Tile = nameof(Tile);
        public const string Component = nameof(Component);
        public const string ComponentType = nameof(ComponentType);
        public const string ComponentProperty = nameof(ComponentProperty);
        public const string EntityID = nameof(EntityID);
    }
    public static class Modules
    {
        public static readonly Atom Fiero = new("fiero");
        public static readonly Atom Script = new("script");
        public static readonly Atom Animation = new("anim");
        public static readonly Atom Sound = new("sound");
        public static readonly Atom Entity = new("entity");
        public static readonly Atom Dialogue = new("dialogue");
        public static readonly Atom Effect = new("effect");
        public static readonly Atom Data = new("data");
        public static readonly Atom Event = new("event");
        public static readonly Atom Random = new("random");
        public static readonly Atom Map = new("map");
    }
}

[TransientDependency]
public partial class FieroLib(IServiceFactory services) : Library
{
    public readonly record struct MapDef(Atom Name, Coord Size);

    public override Atom Module => Modules.Fiero;

    public readonly Dictionary<Atom, MapDef> Maps = new();

    public bool DeclareMap(Atom name, Coord size)
    {
        if (Maps.ContainsKey(name))
            return false;
        Maps[name] = new(name, size);
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
        yield return services.GetInstance<NextRandom>();
        yield return services.GetInstance<SetRandomSeed>();
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
