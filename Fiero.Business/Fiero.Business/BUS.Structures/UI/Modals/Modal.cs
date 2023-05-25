﻿using Fiero.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    public abstract class Modal : ModalWindow
    {
        protected readonly GameResources Resources;
        protected readonly Dictionary<Hotkey, Action> Hotkeys;
        protected event Action Invalidated;
        private bool _dirty;

        protected Modal(GameUI ui, GameResources resources, ModalWindowButton[] buttons, ModalWindowStyles styles = ModalWindowStyles.Default)
            : base(ui, buttons, styles)
        {
            Resources = resources;
            Hotkeys = new Dictionary<Hotkey, Action>();
        }

        protected override LayoutStyleBuilder DefineStyles(LayoutStyleBuilder builder) => builder
            .AddRule<UIControl>(style => style
                .Match(x => x.HasAnyClass("modal-title", "modal-controls", "modal-content"))
                .Apply(x => x.Background.V = UI.Store.Get(Data.UI.DefaultBackground)))
            .AddRule<UIControl>(style => style
                .Match(x => x.HasClass("row-even"))
                .Apply(x => x.Background.V = UI.Store.Get(Data.UI.DefaultBackground).AddRgb(16, 16, 16)))
            ;

        protected override void SetDefaultSize()
        {
            Size.V = UI.Store.Get(Data.UI.PopUpSize);
            Position.V = Size.V / 2;
        }

        protected void Invalidate()
        {
            _dirty = true;
        }


        public override void Open(string title)
        {
            Hotkeys.Clear();
            RegisterHotkeys(Buttons);
            base.Open(title);
            Invalidate();
        }

        protected virtual void RegisterHotkeys(ModalWindowButton[] buttons)
        {
            if (buttons.Any(b => b.ResultType == true))
            {
                Hotkeys.Add(new Hotkey(UI.Store.Get(Data.Hotkeys.Confirm)), () => Close(ModalWindowButton.ImplicitYes));
            }
            if (buttons.Any(b => b.ResultType == false))
            {
                Hotkeys.Add(new Hotkey(UI.Store.Get(Data.Hotkeys.Cancel)), () => Close(ModalWindowButton.ImplicitNo));
            }
        }

        public override void Close(ModalWindowButton buttonPressed)
        {
            base.Close(buttonPressed);
        }

        public override void Draw()
        {
            if (_dirty)
            {
                Invalidated?.Invoke();
                Layout.Invalidate();
                _dirty = false;
            }
            base.Draw();
        }

        public override void Update()
        {
            var shift = UI.Input.IsKeyPressed(VirtualKeys.Shift);
            var ctrl = UI.Input.IsKeyPressed(VirtualKeys.Control);
            var alt = UI.Input.IsKeyPressed(VirtualKeys.Menu);
            foreach (var pair in Hotkeys)
            {
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
            base.Update();
        }
    }
}
