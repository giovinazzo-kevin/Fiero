using Fiero.Core;

namespace Fiero.Business
{
    public readonly struct MoveRelativeAction : IAction
    {
        public readonly Coord Coord;

        public MoveRelativeAction(Coord coord)
        {
            Coord = coord;
        }

        ActionName IAction.Name => ActionName.Move;
        int? IAction.Cost => 1;
    }
}
