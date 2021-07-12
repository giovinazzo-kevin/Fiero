using Fiero.Core;

namespace Fiero.Business
{
    public readonly struct InteractRelativeAction : IAction
    {
        public readonly Coord Coord;

        public InteractRelativeAction(Coord coord)
        {
            Coord = coord;
        }

        ActionName IAction.Name => ActionName.Interact;
        int? IAction.Cost => 100;
    }
}
