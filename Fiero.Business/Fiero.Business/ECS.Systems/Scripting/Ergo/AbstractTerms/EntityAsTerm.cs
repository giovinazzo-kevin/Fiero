using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Ast.Terms.Interfaces;
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
public sealed class EntityAsTerm : IAbstractTerm
{
    // Hack, TODO: figure out a way to do away with this dependency?
    internal static IServiceFactory ServiceFactory { get; set; }
    internal static readonly Dictionary<Atom, Type> TypeMap;
    public static readonly Atom Id = new("id");
    private static readonly ITerm __invalidEntity = new Dict(new Atom("entity"), new[]
    {
        new KeyValuePair<Atom, ITerm>(new Atom("invalid"), new Atom("true"))
    }).CanonicalForm;

    private readonly GameEntities _entities;
    static EntityAsTerm()
    {
        TypeMap = typeof(Entity).Assembly.GetTypes()
            .Where(t => t.IsAssignableTo(typeof(EcsEntity)) && !t.IsAbstract)
            .ToDictionary(x => new Atom(x.Name.ToString().ToErgoCase()));
    }

    private readonly ITerm _simple;
    public readonly int EntityId;
    public readonly Type Type;

    public EntityAsTerm(int entityId, Atom type)
    {
        _entities = ServiceFactory.GetInstance<GameEntities>();
        Type = TypeMap[type];
        EntityId = entityId;
    }

    public Maybe<EcsEntity> GetProxy()
    {
        if (_entities.TryGetProxy(Type, EntityId, out var entity))
        {
            return entity;
        }
        return default;
    }

    private ITerm ToCanonical() => GetProxy()
        .Select(x => TermMarshall.ToTerm(x, Type))
        .GetOr(__invalidEntity);

    public ITerm CanonicalForm => ToCanonical();
    public Signature Signature => _simple.GetSignature();
    public string Explain()
    {
        return _simple.Explain();
    }

    public Maybe<IAbstractTerm> FromCanonicalTerm(ITerm c) => FromCanonical(c).Select(x => (IAbstractTerm)x);
    public static Maybe<EntityAsTerm> FromCanonical(ITerm term)
    {
        if (term.IsAbstract<Dict>().TryGetValue(out var dict)
            && dict.Functor.TryGetA(out var functor)
            && TypeMap.TryGetValue(functor, out _)
            && dict.Dictionary.TryGetValue(Id, out var id)
            && id is Atom a && a.Value is EDecimal d)
        {
            return new EntityAsTerm(d.ToInt32Unchecked(), functor);
        }
        return default;
    }
    public IAbstractTerm Instantiate(InstantiationContext ctx, Dictionary<string, Variable> vars = null)
        => this;
    public IAbstractTerm Substitute(Substitution s)
        => this;
    public Maybe<SubstitutionMap> Unify(IAbstractTerm other)
    {
        if (other is EntityAsTerm e)
            return _simple.Unify(e._simple);
        return default;
    }
}
