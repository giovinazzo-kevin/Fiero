using System;

namespace Fiero.Core
{
    public class ResourceNotFoundException<TEnum> : Exception
        where TEnum : struct, Enum
    {
        public ResourceNotFoundException(TEnum value) 
            : base($"Resource {typeof(TEnum).Name}.{value} was not found")
        {
        }
    }
}
