namespace Fiero.Core
{
    public interface IUIControlProperty
    {
        string Name { get; }
        Type PropertyType { get; }
        object Value { get; set; }
        UIControl Owner { get; }
        bool Propagated { get; }
        bool Inherited { get; }
        bool Invalidating { get; }

        void SetOwner(UIControl newOwner);
    }
}
