using Ergo.Lang;
using Ergo.Lang.Extensions;
using Ergo.Runtime;
using Ergo.Runtime.BuiltIns;
using LightInject;

namespace Fiero.Business;
[SingletonDependency]
public sealed class At(IServiceFactory services) : BuiltIn("", new("at"), 3, ScriptingSystem.FieroModule)
{
    private readonly IServiceFactory _services = services;

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
            else if (!args[0].Matches(out loc))
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
            var (a, f, i) = (0, 0, 0);
            vm.PushChoice(NextActor);
            vm.SetArg(0, args[2]);
            vm.SetArg(1, TermMarshall.ToTerm(cell.Tile));
            ErgoVM.Goals.Unify2(vm);
            void NextActor(ErgoVM vm)
            {
                var A = cell.Actors.ElementAt(a++);
                if (a < cell.Actors.Count)
                    vm.PushChoice(NextActor);
                else
                    vm.PushChoice(NextFeature);
                vm.SetArg(1, new EntityAsTerm(A.Id, A.ErgoType()));
                ErgoVM.Goals.Unify2(vm);
            }
            void NextFeature(ErgoVM vm)
            {
                var F = cell.Features.ElementAt(f++);
                if (f < cell.Features.Count)
                    vm.PushChoice(NextFeature);
                else
                    vm.PushChoice(NextItem);
                vm.SetArg(1, new EntityAsTerm(F.Id, F.ErgoType()));
                ErgoVM.Goals.Unify2(vm);
            }
            void NextItem(ErgoVM vm)
            {
                var F = cell.Items.ElementAt(i++);
                if (i < cell.Items.Count)
                    vm.PushChoice(NextItem);
                vm.SetArg(1, new EntityAsTerm(F.Id, F.ErgoType()));
                ErgoVM.Goals.Unify2(vm);
            }
        };
    }
}
