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
    public static readonly Atom Functor = new("entity");
    private static readonly ITerm __invalidEntity = new Dict(Functor, new[]
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
        _simple = new Complex(Functor, type, new Atom(entityId));
        Type = TypeMap[type];
        EntityId = entityId;
    }

    private ITerm ToCanonical()
    {
        if (_entities.TryGetProxy(Type, EntityId, out var entity))
        {
            return TermMarshall.ToTerm(entity, Type);
        }
        return __invalidEntity;
    }

    public ITerm CanonicalForm => ToCanonical();
    public Signature Signature => _simple.GetSignature();
    public string Explain()
    {
        return _simple.Explain();
    }
    public static bool IsCanonical(Complex c) =>
        c.Functor.Equals(Functor)
        && c.Arity == 2
        && c.Arguments[0] is Atom id
        && id.Value is EDecimal
        && c.Arguments[1] is Atom type
        && TypeMap.ContainsKey(type);

    public Maybe<IAbstractTerm> FromCanonicalTerm(ITerm c) => FromCanonical(c).Select(x => (IAbstractTerm)x);
    public static Maybe<EntityAsTerm> FromSimple(ITerm term)
    {
        if (term is Complex c && IsCanonical(c))
        {
            return new EntityAsTerm(((EDecimal)((Atom)c.Arguments[0]).Value).ToInt32Unchecked(), (Atom)c.Arguments[1]);
        }
        return default;
    }
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
