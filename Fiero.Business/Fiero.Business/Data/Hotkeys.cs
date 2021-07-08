using Fiero.Core;
using SFML.Window;

namespace Fiero.Business
{
    public static partial class Data
    {
        public static class Hotkeys
        {
            public static readonly GameDatum<Keyboard.Key> Cancel = new(nameof(Hotkeys) + nameof(Cancel));
            public static readonly GameDatum<Keyboard.Key> ToggleInventory = new(nameof(Hotkeys) + nameof(ToggleInventory));
        }

    }
}
