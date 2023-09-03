using Fiero.Core;
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

        public bool CanBeClosedImplicitly => Buttons.Any(b => b.ResultType == false)
            || this.Styles.HasFlag(ModalWindowStyles.TitleBar_Close);

        protected static ModalWindowStyles GetDefaultStyles(ModalWindowButton[] buttons)
            => buttons.Any(x => x.ResultType == false)
                ? ModalWindowStyles.Default
                : ModalWindowStyles.Default & ~ModalWindowStyles.TitleBar_Close;

        protected Modal(GameUI ui, GameResources resources, ModalWindowButton[] buttons, ModalWindowStyles? styles = null)
            : base(ui, buttons, styles ?? GetDefaultStyles(buttons))
        {
            Resources = resources;
            Hotkeys = new Dictionary<Hotkey, Action>();
            Data.UI.WindowSize.ValueChanged += WindowSize_ValueChanged;
            void WindowSize_ValueChanged(GameDatumChangedEventArgs<Coord> obj)
            {
                OnLayoutRebuilt(Layout);
            }
        }
        protected override LayoutStyleBuilder DefineStyles(LayoutStyleBuilder builder) => builder
            .AddRule<UIControl>(style => style
                .Match(x => x.HasAnyClass("modal-title", "modal-controls"))
                .Apply(x => x.Background.V = UI.GetColor(ColorName.UIBorder))
                .Apply(x => x.Foreground.V = UI.GetColor(ColorName.UIBackground))
                .Apply(x => x.OutlineColor.V = UI.GetColor(ColorName.UIBorder))
            )
            .AddRule<UIControl>(style => style
                .Match(x => x.HasAnyClass("modal-close"))
                .Apply(x => x.Background.V = UI.GetColor(ColorName.Red))
                .Apply(x => x.Foreground.V = UI.GetColor(ColorName.White))
            )
            .AddRule<UIControl>(style => style
                .Match(x => x.HasAllClasses("row", "row-even"))
                .Apply(x => x.Background.V = UI.GetColor(ColorName.UIBackground).AddRgb(8, 8, 8)))
            .AddRule<UIControl>(style => style
                .Match(x => x.HasAllClasses("row", "row-odd"))
                .Apply(x => x.Background.V = UI.GetColor(ColorName.UIBackground)))
            ;

        protected override void OnLayoutRebuilt(Layout oldValue)
        {
            //base.OnLayoutRebuilt(oldValue);
            var vws = UI.Store.Get(Data.UI.ViewportSize);
            Layout.Size.V = (vws * new Vec(0.8f, 0.6f)).ToCoord();
            Layout.Position.V = vws / 2 - Layout.Size.V / 2;
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
            if (CanBeClosedImplicitly)
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
