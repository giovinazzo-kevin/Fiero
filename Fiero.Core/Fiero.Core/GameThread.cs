namespace Fiero.Core
{
    public static class GameThread
    {
        public static readonly ThreadLocal<bool> IsMainThread = new();
        public static readonly ThreadLocal<bool> IsUIThread = new();
    }
}
