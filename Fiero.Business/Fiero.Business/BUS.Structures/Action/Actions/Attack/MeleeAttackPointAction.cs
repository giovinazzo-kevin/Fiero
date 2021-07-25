using Fiero.Core;

namespace Fiero.Business
{
    public readonly struct MeleeAttackPointAction : IAction
    {
        public readonly Coord Point;
        public readonly Weapon[] Weapons;
        public MeleeAttackPointAction(Coord coord, params Weapon[] weapons)
        {
            Point = coord;
            Weapons = weapons;
        }
        ActionName IAction.Name => ActionName.Attack;
        int? IAction.Cost => 100;
    }
}
