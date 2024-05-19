namespace Fiero.Core
{
    public interface IUIControlResolver
    {
        public Type Type { get; }
        UIControl ResolveUntyed();
    }

    public interface IUIControlResolver<T> : IUIControlResolver
        where T : UIControl
    {
        T Resolve();
        UIControl IUIControlResolver.ResolveUntyed() => Resolve();
    }
}
