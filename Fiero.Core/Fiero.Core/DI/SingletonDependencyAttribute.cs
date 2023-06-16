namespace Fiero.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class SingletonDependencyAttribute : Attribute
    {
        public readonly Type InterfaceType;

        public SingletonDependencyAttribute(Type interfaceType = null)
        {
            InterfaceType = interfaceType;
        }
    }
}
