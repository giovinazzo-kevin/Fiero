using System;

namespace Fiero.Core
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ExitStateAttribute : Attribute { }
}
