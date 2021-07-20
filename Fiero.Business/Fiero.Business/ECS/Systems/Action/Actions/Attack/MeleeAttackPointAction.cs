using Fiero.Core;

namespace Fiero.Business
{
    public readonly struct MeleeAttackPointAction : IAction
    {
        public readonly Coord Point;
        public MeleeAttackPointAction(Coord coord)
        {
            Point = coord;
        }
        ActionName IAction.Name => ActionName.Attack;
        int? IAction.Cost => 100;
    }
}
