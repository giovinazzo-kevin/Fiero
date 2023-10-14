using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Parser;
using LightInject;

namespace Fiero.Business;

public class EntityAsTermParser : IAbstractTermParser<EntityAsTerm>
{
    // IMPORTANT: Load before stdlib parsers, in this case before DictParser.
    // This is because entities ARE dicts but they carry extra baggage (to keep the state synced).
    // A term may only have ONE abstract form, therefore the most complex one must be parsed first.
    public int ParsePriority => -1;
    private static readonly Atom[] _functors = EntityAsTerm.TypeMap.Keys.ToArray();
    public IEnumerable<Atom> FunctorsToIndex => _functors;

    public EntityAsTermParser(IServiceFactory serviceFactory)
    {
        EntityAsTerm.ServiceFactory /*dirty hack*/ = serviceFactory;
    }
    public Maybe<EntityAsTerm> Parse(ErgoParser parser)
        => new DictParser().Parse(parser)
               .Map(EntityAsTerm.FromCanonical);

}