using Fiero.Core;

namespace Fiero.Business
{
    public readonly struct RangedAttackPointAction : IAction
    {
        public readonly Coord Point;
        public readonly Weapon[] Weapons;
        public RangedAttackPointAction(Coord coord, params Weapon[] weapons)
        {
            Point = coord;
            Weapons = weapons;
        }
        ActionName IAction.Name => ActionName.Attack;
        int? IAction.Cost => 100;
    }
}
