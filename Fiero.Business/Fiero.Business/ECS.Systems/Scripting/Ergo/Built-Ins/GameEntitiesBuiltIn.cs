using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Solver.BuiltIns;
using Fiero.Core;

namespace Fiero.Business;

public abstract class GameEntitiesBuiltIn : SolverBuiltIn
{
    public readonly GameDataStore Store;
    public readonly GameEntities Entities;
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
