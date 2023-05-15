namespace Fiero.Business
{
    public partial class ErgoScriptingSystem
    {
        public readonly struct InputAvailableEvent
        {
            public readonly string Chunk;

            public InputAvailableEvent(string chunk)
            {
                Chunk = chunk;
            }
        }
    }
}
