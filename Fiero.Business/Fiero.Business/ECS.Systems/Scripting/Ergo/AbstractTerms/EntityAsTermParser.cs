using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Parser;
using LightInject;

namespace Fiero.Business;

public class EntityAsTermParser : IAbstractTermParser<EntityAsTerm>
{
    private static readonly Atom[] _functors = new Atom[] { EntityAsTerm.Functor };
    public IEnumerable<Atom> FunctorsToIndex => _functors;
    public EntityAsTermParser(IServiceFactory serviceFactory)
    {
        EntityAsTerm.ServiceFactory /*dirty hack*/ = serviceFactory;
    }
    public Maybe<EntityAsTerm> Parse(ErgoParser parser)
        => parser.Complex()
            .Map(c => EntityAsTerm.FromSimple(c));

}