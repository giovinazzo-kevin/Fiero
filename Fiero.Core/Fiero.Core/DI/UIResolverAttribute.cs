namespace Fiero.Core
{
    public class UIResolverAttribute<T> : SingletonDependencyAttribute
        where T : UIControl
    {
        public UIResolverAttribute()
            : base(typeof(IUIControlResolver), typeof(IUIControlResolver<T>))
        {

        }
    }
}
