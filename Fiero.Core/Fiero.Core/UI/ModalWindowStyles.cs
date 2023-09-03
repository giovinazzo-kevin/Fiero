namespace Fiero.Core
{
    [Flags]
    public enum ModalWindowStyles : int
    {
        None = 0,
        Title = 1,
        TitleBar = 2,
        CustomButtons = 4,

        Default = Title | TitleBar | CustomButtons
    }
}
