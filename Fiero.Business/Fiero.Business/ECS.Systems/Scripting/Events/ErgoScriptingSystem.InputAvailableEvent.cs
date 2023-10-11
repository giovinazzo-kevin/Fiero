namespace Fiero.Business
{
    public partial class ScriptingSystem
    {
        public readonly record struct InputAvailableEvent(string Chunk);
    }
}
