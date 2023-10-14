using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using LightInject;
using PeterO.Numbers;

namespace Fiero.Business;

/// <summary>
/// Represents a game entity dynamically as a term, implementing a duality between the following representations:
/// entity(Id, Type)
/// Type { id: Id, component: component_type { ... }, ...  }
/// Additionally guarantees that components always contain the latest state when accessed.
/// </summary>
public sealed class EntityAsTerm : Dict
{
    // Hack, TODO: figure out a way to do away with this dependency?
    internal static IServiceFactory ServiceFactory { get; set; }
    internal static readonly Dictionary<Atom, Type> TypeMap;
    public static readonly Atom Id = new("id");
    private static ITerm GetInvalidEntity(int id, Atom type) => new Dict(type, new[]
    {
        new KeyValuePair<Atom, ITerm>(new Atom("id"), new Atom(id))
    });

    private readonly GameEntities _entities;
    static EntityAsTerm()
    {
        TypeMap = typeof(Entity).Assembly.GetTypes()
            .Where(t => t.IsAssignableTo(typeof(EcsEntity)) && !t.IsAbstract)
            .ToDictionary(x => new Atom(x.Name.ToString().ToErgoCase()));
    }

    public readonly int EntityId;
    public readonly Type Type;
    public readonly Atom TypeAsAtom;

    public EntityAsTerm(int entityId, Atom type)
        : base(type, new[] { new KeyValuePair<Atom, ITerm>(Id, new Atom(entityId)) })
    {
        _entities = ServiceFactory.GetInstance<GameEntities>();
        Type = TypeMap[type];
        TypeAsAtom = type;
        EntityId = entityId;
    }
    private ITerm ToCanonical() => GetProxy()
        .Select(x => TermMarshall.ToTerm(x, Type))
        .GetOr(GetInvalidEntity(EntityId, TypeAsAtom));

    public override Maybe<SubstitutionMap> Unify(ITerm other)
    {
        return base.Unify(other);
    }

    public Maybe<EcsEntity> GetProxy()
    {
        if (_entities.TryGetProxy(Type, EntityId, out var entity))
        {
            return entity;
        }
        return default;
    }
    public static Maybe<EntityAsTerm> FromCanonical(ITerm term)
    {
        if (term is Dict dict
            && dict.Functor.TryGetA(out var functor)
            && TypeMap.TryGetValue(functor, out _)
            && dict.Dictionary.TryGetValue(Id, out var id)
            && id is Atom a && a.Value is EDecimal d)
        {
            return new EntityAsTerm(d.ToInt32Unchecked(), functor);
        }
        return default;
    }
}