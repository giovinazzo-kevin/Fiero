namespace Fiero.Core;

public record class LayoutRef<T> where T : UIControl
{
    private T _ref;
    public T Control
    {
        get => _ref; internal set
        {
            var old = _ref;
            _ref = value;
            ControlChanged?.Invoke(this, old);
        }
    }
    public event Action<LayoutRef<T>, T> ControlChanged;
}
