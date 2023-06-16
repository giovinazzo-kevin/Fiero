namespace Fiero.Core
{
    [Flags]
    public enum ModalWindowStyles : int
    {
        None = 0,
        Title = 1,
        Buttons = 2,

        Default = Title | Buttons
    }
}
