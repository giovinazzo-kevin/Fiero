using Fiero.Core;

namespace Fiero.Business
{
    public readonly struct Hotkey
    {
        public readonly VirtualKeys Key;
        public readonly bool Shift;
        public readonly bool Control;
        public readonly bool Alt;

        public Hotkey(VirtualKeys key, bool shift = false, bool control = false, bool alt = false)
        {
            Key = key;
            Shift = shift;
            Control = control;
            Alt = alt;
        }
    }
}
