using System.Threading;

namespace Fiero.Core
{

    public sealed class OffButton
    {
        private readonly CancellationTokenSource _source;
        public CancellationToken Token => _source.Token;

        public OffButton()
        {
            _source = new CancellationTokenSource();
        }

        /// <summary>
        /// Shuts down the game gently.
        /// </summary>
        public void Press()
        {
            _source.Cancel(false);
        }
    }
}
