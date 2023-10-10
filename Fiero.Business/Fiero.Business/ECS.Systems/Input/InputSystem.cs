using Unconcern.Common;

namespace Fiero.Business
{
    public partial class InputSystem : EcsSystem
    {
        public enum KeyEventType
        {
            Pressed,
            Released,
        }

        public record struct KeyEvent(VirtualKeys Key, KeyEventType Type);

        public readonly SystemRequest<InputSystem, KeyEvent, EventResult> KeyboardEvent;

        public readonly GameInput Input;

        public InputSystem(EventBus bus, GameInput input) : base(bus)
        {
            Input = input;
            KeyboardEvent = new(this, nameof(KeyboardEvent));
        }

        public void Update(TimeSpan t, TimeSpan dt)
        {
            if (Input.IsKeyboardFocusAvailable)
            {
                foreach (var key in Input.KeysPressed)
                {
                    KeyboardEvent.Handle(new(key, KeyEventType.Pressed));
                }
                foreach (var key in Input.KeysReleased)
                {
                    KeyboardEvent.Handle(new(key, KeyEventType.Released));
                }
            }
        }
    }
}
