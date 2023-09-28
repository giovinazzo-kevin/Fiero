using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using Ergo.Solver.BuiltIns;
using System.Reflection;

namespace Fiero.Business;

public abstract class GameEntitiesBuiltIn : SolverBuiltIn
{
    public readonly GameDataStore Store;
    public readonly GameEntities Entities;

    public static readonly IReadOnlyDictionary<string, Type> ProxyableEntityTypes = Assembly.GetExecutingAssembly()
        .GetTypes()
        .Where(t => !t.IsAbstract && t.IsAssignableTo(typeof(Entity)))
        .ToDictionary(x => x.Name.ToErgoCase());
    public static readonly IReadOnlyDictionary<string, Type> ProxyableComponentTypes = Assembly.GetExecutingAssembly()
        .GetTypes()
        .Where(t => !t.IsAbstract && t.IsAssignableTo(typeof(EcsComponent)))
        .ToDictionary(x => x.Name.ToErgoCase());
    public static readonly IReadOnlyDictionary<string, Dictionary<string, PropertyInfo>> ProxyableComponentProperties = Assembly.GetExecutingAssembly()
        .GetTypes()
        .Where(t => !t.IsAbstract && t.IsAssignableTo(typeof(EcsComponent)))
        .ToDictionary(x => x.Name.ToErgoCase(), x => x.GetProperties(BindingFlags.Instance | BindingFlags.Public).ToDictionary(x => x.Name.ToErgoCase()));

    public static readonly MethodInfo TryGetProxy = typeof(GameEntities)
        .GetMethods(BindingFlags.Instance | BindingFlags.Public).Single(x => x.Name.Equals(nameof(GameEntities.TryGetProxy)) && x.IsGenericMethod);

    public static readonly MethodInfo TryGetComponent = typeof(GameEntities)
        .GetMethod(nameof(GameEntities.TryGetComponent), BindingFlags.Instance | BindingFlags.Public);


    public GameEntitiesBuiltIn(string doc, Atom functor, Maybe<int> arity, GameEntities entities, GameDataStore store)
        : base(doc, functor, arity, ErgoScriptingSystem.FieroModule)
    {
        Entities = entities;
        Store = store;
    }

    protected bool TryParseSpecial(string arg, out Entity e)
    {
        e = default;
        return arg switch
        {
            "player" when Entities.TryGetProxy(Store.Get(Data.Player.Id), out e) => true,
            _ => false
        };
    }
}
