namespace Fiero.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class TransientDependencyAttribute(params Type[] interfaceTypes) : Attribute
    {
        public readonly Type[] InterfaceTypes = interfaceTypes;
    }
}
