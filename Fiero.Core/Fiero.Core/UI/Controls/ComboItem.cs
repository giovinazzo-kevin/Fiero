namespace Fiero.Core
{
    public class ComboItem : Label
    {
        public ComboItem(GameInput input) : base(input)
        {
            IsInteractive.V = true;
        }
    }
}
