namespace Fiero.Business
{
    public readonly struct ShootLauncherAtPointAction : IAction
    {
        public readonly Launcher Launcher;
        public readonly Coord Point;

        public ShootLauncherAtPointAction(Launcher launcher, Coord point)
        {
            Launcher = launcher;
            Point = point;
        }

        ActionName IAction.Name => ActionName.Shoot;
        int? IAction.Cost => 100;
    }
}
