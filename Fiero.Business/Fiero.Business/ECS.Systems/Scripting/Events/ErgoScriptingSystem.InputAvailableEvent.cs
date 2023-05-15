namespace Fiero.Business
{
    public partial class ErgoScriptingSystem
    {
        public readonly record struct InputAvailableEvent(string Chunk);
    }
}
