using Ergo.Shell;
using Fiero.Core;

namespace Fiero.Business
{
    [SingletonDependency(typeof(IAsyncInputReader))]
    public sealed class ErgoInputReader : IAsyncInputReader
    {
        public readonly KeyboardInputReader Reader;
        public ErgoInputReader(KeyboardInputReader r)
        {
            Reader = r;
        }

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
