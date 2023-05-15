namespace Fiero.Core
{
    public class Button : Label
    {
        public Button(GameInput input) : base(input)
        {
            IsInteractive.V = true;
        }
    }
}
