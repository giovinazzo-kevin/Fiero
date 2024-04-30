namespace Fiero.Business
{
    public readonly struct MoveRelativeAction : IAction
    {
        public readonly Coord Coord;

        public readonly int MoveDelay;

        public MoveRelativeAction(Coord coord, int moveDelay)
        {
            Coord = coord;
            MoveDelay = moveDelay;
        }

        ActionName IAction.Name => ActionName.Move;
        int? IAction.Cost => MoveDelay;
    }
}
