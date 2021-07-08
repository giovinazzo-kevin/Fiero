using Fiero.Core;
using SFML.Graphics;
using SFML.Window;
using System;
using System.Collections.Generic;

namespace Fiero.Business
{
    public abstract class Modal : ModalWindow
    {
        protected readonly Dictionary<Hotkey, Action> Hotkeys;
        
        private void WindowSize_ValueChanged(GameDatumChangedEventArgs<Coord> obj)
        {
            Layout.Size.V = obj.NewValue;
        }

        protected Modal(GameUI ui) : base(ui)
        {
            Hotkeys = new Dictionary<Hotkey, Action>();
            Hotkeys.Add(new Hotkey(UI.Store.Get(Data.Hotkeys.Cancel)), () => Close(ModalWindowButtons.None));
            Data.UI.WindowSize.ValueChanged += WindowSize_ValueChanged;
        }

        public override void Open(string title, ModalWindowButtons buttons)
        {
            base.Open(title, buttons);
            Layout.Size.V = UI.Store.Get(Data.UI.WindowSize);
        }

        public override void Close(ModalWindowButtons buttonPressed)
        {
            Data.UI.WindowSize.ValueChanged -= WindowSize_ValueChanged;
            base.Close(buttonPressed);
        }

        public override void Update(RenderWindow win, float t, float dt)
        {
            var shift = UI.Input.IsKeyPressed(Keyboard.Key.LShift)
                      ^ UI.Input.IsKeyPressed(Keyboard.Key.RShift);
            var ctrl  = UI.Input.IsKeyPressed(Keyboard.Key.LControl)
                      ^ UI.Input.IsKeyPressed(Keyboard.Key.RControl);
            var alt   = UI.Input.IsKeyPressed(Keyboard.Key.LAlt)
                      ^ UI.Input.IsKeyPressed(Keyboard.Key.RAlt);
            foreach (var pair in Hotkeys) {
                if (!UI.Input.IsKeyPressed(pair.Key.Key))
                    continue;
                if (pair.Key.Shift && !shift)
                    continue;
                if (pair.Key.Control && !ctrl)
                    continue;
                if (pair.Key.Alt && !alt)
                    continue;
                pair.Value();
            }
            base.Update(win, t, dt);
        }
    }
}
