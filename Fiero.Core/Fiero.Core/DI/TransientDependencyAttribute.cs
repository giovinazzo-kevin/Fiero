namespace Fiero.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class TransientDependencyAttribute : Attribute
    {
        public readonly Type InterfaceType;

        public TransientDependencyAttribute(Type interfaceType = null)
        {
            InterfaceType = interfaceType;
        }
    }
}
