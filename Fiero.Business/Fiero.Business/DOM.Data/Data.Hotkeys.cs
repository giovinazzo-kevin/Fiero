using Fiero.Core;
using SFML.Window;

namespace Fiero.Business
{
    public static partial class Data
    {
        public static class Hotkeys
        {
            public static readonly GameDatum<Keyboard.Key> Cancel = new(nameof(Hotkeys) + nameof(Cancel));
            public static readonly GameDatum<Keyboard.Key> Confirm = new(nameof(Hotkeys) + nameof(Confirm));
            public static readonly GameDatum<Keyboard.Key> Modifier = new(nameof(Hotkeys) + nameof(Modifier));
            public static readonly GameDatum<Keyboard.Key> Inventory = new(nameof(Hotkeys) + nameof(Inventory));
            public static readonly GameDatum<Keyboard.Key> Interact = new(nameof(Hotkeys) + nameof(Interact));
            public static readonly GameDatum<Keyboard.Key> Look = new(nameof(Hotkeys) + nameof(Look));
            public static readonly GameDatum<Keyboard.Key> FireWeapon = new(nameof(Hotkeys) + nameof(FireWeapon));
            public static readonly GameDatum<Keyboard.Key> QuickCast1 = new(nameof(Hotkeys) + nameof(QuickCast1));
            public static readonly GameDatum<Keyboard.Key> QuickCast2 = new(nameof(Hotkeys) + nameof(QuickCast2));
            public static readonly GameDatum<Keyboard.Key> QuickCast3 = new(nameof(Hotkeys) + nameof(QuickCast3));
            public static readonly GameDatum<Keyboard.Key> QuickCast4 = new(nameof(Hotkeys) + nameof(QuickCast4));
            public static readonly GameDatum<Keyboard.Key> MoveNW = new(nameof(Hotkeys) + nameof(MoveNW));
            public static readonly GameDatum<Keyboard.Key> MoveN = new(nameof(Hotkeys) + nameof(MoveN));
            public static readonly GameDatum<Keyboard.Key> MoveNE = new(nameof(Hotkeys) + nameof(MoveNE));
            public static readonly GameDatum<Keyboard.Key> MoveE = new(nameof(Hotkeys) + nameof(MoveE));
            public static readonly GameDatum<Keyboard.Key> MoveSE = new(nameof(Hotkeys) + nameof(MoveSE));
            public static readonly GameDatum<Keyboard.Key> MoveS = new(nameof(Hotkeys) + nameof(MoveS));
            public static readonly GameDatum<Keyboard.Key> MoveSW = new(nameof(Hotkeys) + nameof(MoveSW));
            public static readonly GameDatum<Keyboard.Key> MoveW = new(nameof(Hotkeys) + nameof(MoveW));
            public static readonly GameDatum<Keyboard.Key> Wait = new(nameof(Hotkeys) + nameof(Wait));
            public static readonly GameDatum<Keyboard.Key> ToggleZoom = new(nameof(Hotkeys) + nameof(ToggleZoom));
        }

    }
}
