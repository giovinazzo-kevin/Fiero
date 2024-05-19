using Ergo.Lang;
using Ergo.Lang.Extensions;
using Ergo.Runtime;
using Ergo.Runtime.BuiltIns;
using LightInject;

namespace Fiero.Business;
[SingletonDependency]
public sealed class At(IServiceFactory services) : BuiltIn("", new("at"), 2, FieroLib.Modules.Fiero)
{
    private readonly IServiceFactory _services = services;
    private readonly GameEntities _entities = services.GetInstance<GameEntities>();

    public override ErgoVM.Op Compile()
    {
        return vm =>
        {
            Location loc;
            var args = vm.Args;
            if (args[0].IsEntity<PhysicalEntity>().TryGetValue(out var entity))
            {
                loc = entity.Location();
            }
            else if (!args[0].Match(out loc))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, nameof(Location), args[0]);
                return;
            }
            var systems = _services.GetInstance<MetaSystem>();
            var cell = systems.Get<DungeonSystem>().GetCellAt(loc.FloorId, loc.Position);
            if (cell is null)
            {
                vm.Fail();
                return;
            }
            var lhs = args[1];
            var (a, f, i) = (0, 0, 0);
            if (cell.Actors.Count != 0)
                vm.PushChoice(NextActor);
            else if (cell.Features.Count != 0)
                vm.PushChoice(NextFeature);
            else if (cell.Items.Count != 0)
                vm.PushChoice(NextItem);
            vm.SetArg(0, lhs);
            vm.SetArg(1, TermMarshall.ToTerm(cell.Tile));
            ErgoVM.Goals.Unify2(vm);
            void NextActor(ErgoVM vm)
            {
                var A = cell.Actors.ElementAt(a++);
                if (a < cell.Actors.Count)
                    vm.PushChoice(NextActor);
                else if (cell.Features.Count != 0)
                    vm.PushChoice(NextFeature);
                else if (cell.Items.Count != 0)
                    vm.PushChoice(NextItem);
                vm.SetArg(0, lhs);
                vm.SetArg(1, new EntityAsTerm(A.Id, A.ErgoType(), _entities));
                ErgoVM.Goals.Unify2(vm);
            }
            void NextFeature(ErgoVM vm)
            {
                var F = cell.Features.ElementAt(f++);
                if (f < cell.Features.Count)
                    vm.PushChoice(NextFeature);
                else if (cell.Items.Count != 0)
                    vm.PushChoice(NextItem);
                vm.SetArg(0, lhs);
                vm.SetArg(1, new EntityAsTerm(F.Id, F.ErgoType(), _entities));
                ErgoVM.Goals.Unify2(vm);
            }
            void NextItem(ErgoVM vm)
            {
                var F = cell.Items.ElementAt(i++);
                if (i < cell.Items.Count)
                    vm.PushChoice(NextItem);
                vm.SetArg(0, lhs);
                vm.SetArg(1, new EntityAsTerm(F.Id, F.ErgoType(), _entities));
                ErgoVM.Goals.Unify2(vm);
            }
        };
    }
}
