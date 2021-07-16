using System;

namespace Fiero.Core
{
    public interface IUIControlProperty
    {
        string Name { get; }
        Type PropertyType { get; }
        object Value { get; set; }
        UIControl Owner { get; }

        void SetOwner(UIControl newOwner);
    }
}
