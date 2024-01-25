namespace Fiero.Business
{
    public readonly struct ShootLauncherAtOtherAction : IAction
    {
        public readonly Launcher Launcher;
        public readonly Actor Victim;

        public ShootLauncherAtOtherAction(Launcher launcher, Actor actor)
        {
            Launcher = launcher;
            Victim = actor;
        }

        ActionName IAction.Name => ActionName.Shoot;
        int? IAction.Cost => 100;
    }
}
