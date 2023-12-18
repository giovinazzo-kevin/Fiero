using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Runtime;

namespace Fiero.Business;

// NOTE: Possibly superseded by EntityAsTerm, though it could still be useful.

[SingletonDependency]
public sealed class CastEntity(GameEntities entities, GameDataStore store) : GameEntitiesBuiltIn("", new("cast_entity"), 3, entities, store)
{
    public override ErgoVM.Op Compile()
    {
        return vm =>
        {
            var arguments = vm.Args;
            var (entityId, proxyType, cast) = (arguments[0], arguments[1], arguments[2]);
            if (!proxyType.IsGround || !ProxyableEntityTypes.TryGetValue(proxyType.Explain(), out var type))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, FieroLib.Types.EntityType, proxyType.Explain());
                return;
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
                        Unify(vm, cast, (EcsEntity)tryGetProxyArgs[1]);
                    else vm.Fail();
                }
                else
                {
                    vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, FieroLib.Types.EntityID, entityId);
                    return;
                }
            }
            else
            {
                int i = 0;
                var entityIds = Entities.GetEntities().ToArray();
                vm.PushChoice(NextSolution);
                NextSolution(vm);
                void NextSolution(ErgoVM vm)
                {
                    var id = entityIds[i++];
                    var tryGetProxyArgs = new object[] { id, Activator.CreateInstance(type) };
                    if ((bool)TryGetProxy.MakeGenericMethod(type).Invoke(Entities, tryGetProxyArgs))
                        Unify(vm, cast, (EcsEntity)tryGetProxyArgs[1]);
                    else vm.Fail();
                }
            }
        };

        static void Unify(ErgoVM vm, ITerm cast, EcsEntity entity)
        {
            vm.SetArg(0, cast);
            vm.SetArg(1, new EntityAsTerm(entity.Id, entity.ErgoType()));
            ErgoVM.Goals.Unify2(vm);
        }
    }
}
