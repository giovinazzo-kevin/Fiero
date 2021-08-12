using Fiero.Core;

namespace Fiero.Business
{
    public readonly struct ThrowItemAtPointAction : IAction
    {
        public readonly Coord Point;
        public readonly Throwable Item;
        public ThrowItemAtPointAction(Coord coord, Throwable item)
        {
            Point = coord;
            Item = item;
        }
        ActionName IAction.Name => ActionName.Throw;
        int? IAction.Cost => 100;
    }
}
