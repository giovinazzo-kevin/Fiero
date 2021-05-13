namespace Fiero.Core
{
    public interface IUIControlResolver<T>
        where T : UIControl
    {
        T Resolve(Coord position, Coord size);
    }
}
