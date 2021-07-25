using SFML.Window;

namespace Fiero.Business
{
    public readonly struct Hotkey
    {
        public readonly Keyboard.Key Key;
        public readonly bool Shift;
        public readonly bool Control;
        public readonly bool Alt;

        public Hotkey(Keyboard.Key key, bool shift = false, bool control = false, bool alt = false)
        {
            Key = key;
            Shift = shift;
            Control = control;
            Alt = alt;
        }
    }
}
