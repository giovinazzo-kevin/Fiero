using SFML.Graphics;

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

        protected override bool IsMaximized { get; set; }
        public bool IsResponsive { get; set; } = true;

        protected Modal(GameUI ui, GameResources resources, ModalWindowButton[] buttons, ModalWindowStyles? styles = null)
            : base(ui, buttons, styles ?? GetDefaultStyles(buttons))
        {
            Resources = resources;
            Hotkeys = new Dictionary<Hotkey, Action>();
            Data.View.ViewportSize.ValueChanged += ViewportSize_ValueChanged;
            void ViewportSize_ValueChanged(GameDatumChangedEventArgs<Coord> obj)
            {
                if (IsResponsive && Layout != null)
                    Layout.Size.V += obj.NewValue - obj.OldValue;
            }
        }
        protected override LayoutThemeBuilder DefineStyles(LayoutThemeBuilder builder) => builder
            .Rule<UIControl>(style => style
                .Match(x => x.HasAnyClass("modal-title", "modal-controls"))
                .Apply(x => x.Background.V = UI.GetColor(ColorName.UIBorder))
                .Apply(x => x.Foreground.V = UI.GetColor(ColorName.UIBackground))
                .Apply(x => x.OutlineColor.V = UI.GetColor(ColorName.UIBorder))
            )
            .Rule<UIControl>(style => style
                .Match(x => x.HasAnyClass("modal-close"))
                .Apply(x => x.Background.V = UI.GetColor(ColorName.Red))
                .Apply(x => x.Foreground.V = UI.GetColor(ColorName.White))
            )
            .Rule<UIControl>(style => style
                .Match(x => x.HasAnyClass("modal-maximize"))
                .Apply(x => x.Background.V = UI.GetColor(ColorName.Blue))
                .Apply(x => x.Foreground.V = UI.GetColor(ColorName.White))
            )
            ;

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

        public override void Maximize()
        {
            Layout.Size.V = UI.Store.Get(CoreData.View.WindowSize);
            Layout.Position.V = Coord.Zero;
            IsMaximized = true;
        }

        public override void Minimize()
        {
            var vws = UI.Store.Get(Data.View.ViewportSize);
            Layout.Size.V = (vws * new Vec(0.8f, 0.6f)).ToCoord();
            Layout.Position.V = vws / 2 - Layout.Size.V / 2;
            IsMaximized = false;
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

        public override void Draw(RenderTarget target, RenderStates states)
        {
            if (_dirty)
            {
                Invalidated?.Invoke();
                Layout.Invalidate();
                _dirty = false;
            }
            base.Draw(target, states);
        }

        public override void Update(TimeSpan t, TimeSpan dt)
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
            base.Update(t, dt);
        }
    }
}
