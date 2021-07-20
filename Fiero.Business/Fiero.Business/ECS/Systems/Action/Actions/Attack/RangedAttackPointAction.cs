using Fiero.Core;

namespace Fiero.Business
{
    public readonly struct RangedAttackPointAction : IAction
    {
        public readonly Coord Point;
        public RangedAttackPointAction(Coord coord)
        {
            Point = coord;
        }
        ActionName IAction.Name => ActionName.Attack;
        int? IAction.Cost => 100;
    }
}
