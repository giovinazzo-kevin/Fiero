namespace Fiero.Core
{
    [Flags]
    public enum ModalWindowStyles : int
    {
        None = 0,
        Title = 1,
        TitleBar_Close = 2,
        CustomButtons = 128,

        Default = Title | TitleBar_Close | CustomButtons
    }
}
