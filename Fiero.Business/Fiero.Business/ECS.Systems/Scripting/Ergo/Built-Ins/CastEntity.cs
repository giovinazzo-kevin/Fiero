﻿using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Exceptions;
using Ergo.Lang.Extensions;
using Ergo.Solver;
using Ergo.Solver.BuiltIns;
using System.Collections.Immutable;

namespace Fiero.Business;

// NOTE: Possibly superseded by EntityAsTerm, though it could still be useful.

[SingletonDependency]
public sealed class CastEntity : GameEntitiesBuiltIn
{
    public CastEntity(GameEntities entities, GameDataStore store)
        : base("", new("cast_entity"), 3, entities, store)
    {
    }

    public override IEnumerable<Evaluation> Apply(SolverContext solver, SolverScope scope, ImmutableArray<ITerm> arguments)
    {
        var (entityId, proxyType, cast) = (arguments[0], arguments[1], arguments[2]);
        if (!proxyType.IsGround || !ProxyableEntityTypes.TryGetValue(proxyType.Explain(), out var type))
        {
            yield return ThrowFalse(scope, SolverError.ExpectedTermOfTypeAt, FieroLib.Types.EntityType, proxyType.Explain());
            yield break;
        }
        if (entityId.IsGround)
        {
            var expl = entityId.Explain();
            var maybeId = Maybe<int>.None;
            if (TryParseSpecial(expl, out var special))
                maybeId = special.Id;
            else if (int.TryParse(expl, out var id_))
                maybeId = id_;
            else if (entityId is Dict dict && dict.Dictionary.TryGetValue(new("id"), out var match) && int.TryParse(match.Explain(), out id_))
                maybeId = id_;
            if (maybeId.TryGetValue(out var id))
            {
                var tryGetProxyArgs = new object[] { id, Activator.CreateInstance(type), false };
                if ((bool)TryGetProxy.MakeGenericMethod(type).Invoke(Entities, tryGetProxyArgs))
                {
                    yield return Unify((EcsEntity)tryGetProxyArgs[1]);
                    yield break;
                }
                yield return False();
                yield break;
            }
            yield return ThrowFalse(scope, SolverError.ExpectedTermOfTypeAt, FieroLib.Types.EntityID, entityId);
            yield break;
        }
        var any = false;
        foreach (var id in Entities.GetEntities())
        {
            var tryGetProxyArgs = new object[] { id, Activator.CreateInstance(type) };
            if ((bool)TryGetProxy.MakeGenericMethod(type).Invoke(Entities, tryGetProxyArgs))
            {
                var ret = Unify((EcsEntity)tryGetProxyArgs[1]);
                if (!ret.Result.Equals(WellKnown.Literals.False))
                {
                    yield return ret;
                    any = true;
                }
            }
        }
        if (!any)
            yield return False();
        yield break;

        Evaluation Unify(EcsEntity entity)
        {
            var term = new EntityAsTerm(entity.Id, entity.ErgoType());
            if (cast.Unify(term).TryGetValue(out var subs))
            {
                return True(subs);
            }
            return False();
        }
    }
}
