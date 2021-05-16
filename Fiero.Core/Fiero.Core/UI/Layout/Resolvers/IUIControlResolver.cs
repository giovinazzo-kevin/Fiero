namespace Fiero.Core
{
    public interface IUIControlResolver<T>
        where T : UIControl
    {
        T Resolve(LayoutGrid dom);
    }
}
