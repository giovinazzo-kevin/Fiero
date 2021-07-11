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
        protected event Action Invalidated;

        protected Modal(GameUI ui) : base(ui)
        {
            Hotkeys = new Dictionary<Hotkey, Action>();
            Hotkeys.Add(new Hotkey(UI.Store.Get(Data.Hotkeys.Cancel)), () => Close(ModalWindowButtons.ImplicitNo));
            Hotkeys.Add(new Hotkey(UI.Store.Get(Data.Hotkeys.Confirm)), () => Close(ModalWindowButtons.ImplicitYes));
            Data.UI.WindowSize.ValueChanged += OnWindowSizeChanged;
        }
        
        protected virtual void OnWindowSizeChanged(GameDatumChangedEventArgs<Coord> obj)
        {
            Layout.Size.V = obj.NewValue;
            Invalidate();
        }

        protected override LayoutStyleBuilder DefineStyles(LayoutStyleBuilder builder) => builder
            .AddRule<UIControl>(style => style
                .Match(x => x.HasAnyClass("modal-title", "modal-controls", "modal-content"))
                .Apply(x => x.Background.V = UI.Store.Get(Data.UI.DefaultBackground)))
            .AddRule<UIControl>(style => style
                .Match(x => x.HasClass("row-even"))
                .Apply(x => x.Background.V = UI.Store.Get(Data.UI.DefaultBackground).AddRgb(16, 16, 16)))
            ;

        protected void Invalidate() => Invalidated?.Invoke();

        public override void Open(string title, ModalWindowButtons buttons)
        {
            base.Open(title, buttons);
            BeforePresentation();
            Invalidate();
        }

        protected virtual void BeforePresentation()
        {
            Layout.Size.V = UI.Store.Get(Data.UI.WindowSize);
        }

        public override void Close(ModalWindowButtons buttonPressed)
        {
            Data.UI.WindowSize.ValueChanged -= OnWindowSizeChanged;
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
