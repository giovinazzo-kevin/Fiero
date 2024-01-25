namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly record struct LauncherShotEvent(Actor Actor, Actor Victim, Coord Position, Launcher Launcher);
    }
}
