namespace Fiero.Business
{
    public class Launcher : Weapon
    {
        [RequiredComponent]
        public LauncherComponent LauncherProperties { get; private set; }
    }
}
