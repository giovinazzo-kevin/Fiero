using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using Ergo.Solver;
using Ergo.Solver.BuiltIns;
using Fiero.Core;
using System.Collections.Generic;

namespace Fiero.Business;

public sealed class UpdateComponent : GameEntitiesBuiltIn
{
    public UpdateComponent(GameEntities entities, GameDataStore store)
        : base("", new("update_component"), 2, entities, store)
    {
    }

    public override IEnumerable<Evaluation> Apply(SolverContext solver, SolverScope scope, ITerm[] arguments)
    {
        var (compare, newValue) = (arguments[0], arguments[1]);
        if (arguments[0].IsAbstract<Dict>().TryGetValue(out var dict) && dict.Signature.Tag.TryGetValue(out var tag))
        {
            if (ProxyableComponentTypes.TryGetValue(tag.Explain(), out var type))
            {
                var compareProxy = TermMarshall.FromTerm(arguments[0], type, TermMarshalling.Named);
                var newValueProxy = TermMarshall.FromTerm(arguments[0], type, TermMarshalling.Named);
            }
        }
        yield break;
    }
}
