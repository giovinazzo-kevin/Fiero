namespace Fiero.Business;

//public static class EntityProxy
//{
//    public static EntityProxy<T> From<T>(T entity)
//        where T : EcsEntity
//    {
//        return new(entity.Id, entity);
//    }
//}

///// <summary>
///// Marshalls an entity by only passing its id in order to minimize overhead, and so that EntityAsTerm can later hydrate it into a full entity.
///// </summary>
//public record struct EntityProxy<T>(int EntityId, Maybe<T> Value) : IErgoMarshalling<EntityProxy<T>>
//    where T : EcsEntity
//{
//    private readonly ITerm _canonical = new EntityAsTerm(EntityId, new Atom(typeof(T).Name.ToErgoCase())).CanonicalForm;
//    public ITerm ToTerm() => _canonical;
//    public EntityProxy<T> FromTerm(ITerm term)
//    {
//        if (term.IsAbstract<EntityAsTerm>().TryGetValue(out var entity))
//        {
//            return new(EntityId, entity.GetProxy().Select(x => (T)x));
//        }
//        throw new InvalidOperationException();
//    }
//}
