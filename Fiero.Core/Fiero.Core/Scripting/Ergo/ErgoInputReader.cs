using Ergo.Shell;

namespace Fiero.Core
{
    public sealed class ErgoInputReader(KeyboardInputReader r) : IAsyncInputReader
    {
        public readonly KeyboardInputReader Reader = r;

        public bool Blocking { get; private set; }

        public char ReadChar(bool intercept = false)
        {
            Blocking = true;
            var ret = Reader.ReadCharBlocking(intercept);
            Blocking = false;
            return ret;
        }
    }
}
