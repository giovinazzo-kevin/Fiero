using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Ast.Terms.Interfaces;
using Ergo.Lang.Extensions;
using LightInject;
using PeterO.Numbers;

namespace Fiero.Business;

/// <summary>
/// Represents a game entity dynamically as a term, automatically refreshing it in-between accesses. It has the form of a dictionary:
/// Type { id: Id, component: component_type { ... }, ...  }
/// Clearly, this is impure and mutable by all counts, but it simplifies scripts that act on entities a great deal.
/// </summary>
public sealed class EntityAsTerm : Dict
{
    // Hack, TODO: figure out a way to do away with this dependency?
    internal static IServiceFactory ServiceFactory { get; set; }
    internal static readonly Dictionary<Atom, Type> TypeMap;
    public static readonly Atom Key_Id = new("id");
    public static readonly Atom Key_Invalid = new("invalid");
    public static ITerm GetInvalidEntity(int id, Atom type) => new Dict(type, new[]
    {
        new KeyValuePair<Atom, ITerm>(Key_Id, new Atom(id)),
        new KeyValuePair<Atom, ITerm>(Key_Invalid, new Atom(true))
    });
    public static ITerm GetEntityStub(int id, Atom type) => new Dict(type, new[]
    {
        new KeyValuePair<Atom, ITerm>(Key_Id, new Atom(id))
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
    private EcsEntity _proxy;

    public EntityAsTerm(int entityId, Atom type)
        : base(type, new[] { new KeyValuePair<Atom, ITerm>(Key_Invalid, new Atom(entityId)) })
    {
        _entities = ServiceFactory.GetInstance<GameEntities>();
        Type = TypeMap[type];
        TypeAsAtom = type;
        EntityId = entityId;
        Refresh();
    }

    public Maybe<T> GetProxy<T>()
        where T : EcsEntity
    => GetProxy().Map(x => x is T t ? Maybe.Some(t) : default);

    public Maybe<EcsEntity> GetProxy()
    {
        if (_proxy != null && _proxy.TryRefresh(EntityId))
        {
            return _proxy;
        }
        if (_entities.TryGetProxy(Type, EntityId, out var entity))
        {
            _proxy = entity;
            return entity;
        }
        return default;
    }
    private void Refresh()
    {
        var proxy = GetProxy()
            .Select(x => TermMarshall.ToTerm(x, Type))
            .GetOr(GetInvalidEntity(EntityId, TypeAsAtom));
        if (proxy is Dict d)
        {
            Dictionary = Dictionary.Clear().AddRange(d.Dictionary);
            KeyValuePairs = BuildKVPs();
            Argument = new Set(KeyValuePairs);
            CanonicalForm = BuildCanonical();
        }
    }

    public override Maybe<SubstitutionMap> Unify(ITerm other)
    {
        Refresh();
        return base.Unify(other);
    }

    public override AbstractTerm Instantiate(InstantiationContext ctx, Dictionary<string, Variable> vars = null)
    {
        Refresh();
        return this;
    }

    public override AbstractTerm Substitute(Substitution s)
    {
        return this;
    }

    public static Maybe<EntityAsTerm> FromCanonical(ITerm term)
    {
        if (term is Dict dict
            && dict.Functor.TryGetA(out var functor)
            && TypeMap.TryGetValue(functor, out _)
            && dict.Dictionary.TryGetValue(Key_Invalid, out var id)
            && id is Atom a && a.Value is EDecimal d)
        {
            return new EntityAsTerm(d.ToInt32Unchecked(), functor);
        }
        return default;
    }
}