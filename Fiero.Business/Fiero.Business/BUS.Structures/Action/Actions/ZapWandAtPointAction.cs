namespace Fiero.Business
{
    public readonly struct ZapWandAtPointAction : IAction
    {
        public readonly Wand Wand;
        public readonly Coord Point;

        public ZapWandAtPointAction(Wand wand, Coord point)
        {
            Wand = wand;
            Point = point;
        }

        ActionName IAction.Name => ActionName.Zap;
        int? IAction.Cost => 100;
    }
}
