namespace Fiero.Business
{
    public readonly struct ThrowItemAtPointAction(Coord coord, Projectile item) : IAction
    {
        public readonly Coord Point = coord;
        public readonly Projectile Item = item;

        ActionName IAction.Name => ActionName.Throw;
        int? IAction.Cost => 100;
    }
}
