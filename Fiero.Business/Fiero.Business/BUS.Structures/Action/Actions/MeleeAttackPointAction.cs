using Fiero.Core;

namespace Fiero.Business
{
    public readonly struct MeleeAttackPointAction : IAction
    {
        public readonly Coord Point;
        public readonly Weapon[] Weapons;
        public MeleeAttackPointAction(Coord coord, Weapon[] weapons)
        {
            Point = coord;
            Weapons = weapons;
        }
        ActionName IAction.Name => ActionName.MeleeAttack;
        int? IAction.Cost => 100;
    }
}
