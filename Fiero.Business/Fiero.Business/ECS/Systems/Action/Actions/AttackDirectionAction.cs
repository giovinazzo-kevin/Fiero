using Fiero.Core;

namespace Fiero.Business
{
    public readonly struct AttackDirectionAction : IAction
    {
        public readonly Coord Coord;
        public AttackDirectionAction(Coord coord)
        {
            Coord = coord;
        }
        ActionName IAction.Name => ActionName.Attack;
        int? IAction.Cost => 10;
    }
}
