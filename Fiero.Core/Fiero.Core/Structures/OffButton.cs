namespace Fiero.Core.Structures
{
    [SingletonDependency]
    public sealed class OffButton
    {
        private readonly CancellationTokenSource _source;
        public CancellationToken Token => _source.Token;
        public event Action<OffButton> Pressed;
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
            Pressed?.Invoke(this);
        }
    }
}
