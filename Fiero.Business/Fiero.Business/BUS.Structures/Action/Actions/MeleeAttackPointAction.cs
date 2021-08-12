using Fiero.Core;

namespace Fiero.Business
{
    public readonly struct MeleeAttackPointAction : IAction
    {
        public readonly Coord Point;
        public readonly Weapon Weapon;
        public MeleeAttackPointAction(Coord coord, Weapon weapon)
        {
            Point = coord;
            Weapon = weapon;
        }
        ActionName IAction.Name => ActionName.MeleeAttack;
        int? IAction.Cost => 100;
    }
}
