using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Exceptions;
using Ergo.Lang.Extensions;
using Ergo.Solver;
using Ergo.Solver.BuiltIns;
using Fiero.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fiero.Business;

[SingletonDependency]
public sealed class CastEntity : GameEntitiesBuiltIn
{
    public static readonly IReadOnlyDictionary<string, Type> ProxyableTypes = Assembly.GetExecutingAssembly()
        .GetTypes()
        .Where(t => !t.IsAbstract && t.IsAssignableTo(typeof(Entity)))
        .ToDictionary(x => x.Name.ToErgoCase());
    public static readonly MethodInfo TryGetProxy = typeof(GameEntities)
        .GetMethod(nameof(GameEntities.TryGetProxy), BindingFlags.Instance | BindingFlags.Public);

    public CastEntity(GameEntities entities, GameDataStore store)
        : base("", new("cast_entity"), 3, entities, store)
    {
    }

    public override IEnumerable<Evaluation> Apply(SolverContext solver, SolverScope scope, ITerm[] arguments)
    {
        var (entityId, proxyType, cast) = (arguments[0], arguments[1], arguments[2]);
        if (!proxyType.IsGround || !ProxyableTypes.TryGetValue(proxyType.Explain(), out var type))
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
            if (maybeId.TryGetValue(out var id))
            {
                var tryGetProxyArgs = new object[] { id, Activator.CreateInstance(type) };
                if ((bool)TryGetProxy.MakeGenericMethod(type).Invoke(Entities, tryGetProxyArgs))
                {
                    yield return Unify(tryGetProxyArgs[1]);
                    yield break;
                }
                yield return False();
                yield break;
            }
            yield return ThrowFalse(scope, SolverError.ExpectedTermOfTypeAt, FieroLib.Types.EntityID, entityId);
            yield break;
        }
        foreach (var id in Entities.GetEntities())
        {
            var tryGetProxyArgs = new object[] { id, Activator.CreateInstance(type) };
            if ((bool)TryGetProxy.MakeGenericMethod(type).Invoke(Entities, tryGetProxyArgs))
            {
                var ret = Unify(tryGetProxyArgs[1]);
                yield return ret;
                if (ret.Result.Equals(WellKnown.Literals.False))
                    yield break;
            }
        }
        yield break;

        Evaluation Unify(object entity)
        {
            var term = TermMarshall.ToTerm(entity, type);
            if (cast.Unify(term).TryGetValue(out var subs))
            {
                return True(subs);
            }
            return False();
        }
    }
}
