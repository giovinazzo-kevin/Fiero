using Fiero.Business.BUS.Structures.UI.Widgets;


using SFML.Graphics;

namespace Fiero.Business
{

    /// <summary>
    /// A window that lays out the main user interface, making it responsive.
    /// </summary>
    [SingletonDependency]
    public class MainSceneWindow : UIWindow
    {
        public readonly LogBox LogBox;
        public readonly MiniMap MiniMap;
        public readonly Viewport Viewport;
        public readonly StatBar HP;
        public readonly StatBar MP;
        public readonly StatBar XP;
        public readonly QuickBar QuickBar;
        public readonly EquipmentDisplay EquipmentDisplay;

        protected Header PlayerNameLabel { get; private set; }
        protected Label CurrentTurnLabel { get; private set; }
        protected Label CurrentPlaceLabel { get; private set; }
        protected Label CurrentPosLabel { get; private set; }
        protected Label RngSeedLabel { get; private set; }

        protected readonly ActionSystem ActionSystem;

        const int SideBarWidth = 248;
        const int BottomBarHeight = 172;

        public MainSceneWindow(
            GameUI ui,
            LogBox logs, MiniMap miniMap, StatBar hp, StatBar mp, StatBar xp, QuickBar quickbar, EquipmentDisplay equipmentDisplay,
            ActionSystem act, GameResources res, GameLoop loop)
            : base(ui)
        {
            LogBox = logs;
            MiniMap = miniMap;
            HP = hp; HP.Stat.V = nameof(HP);
            HP.EnableDragging = false;
            HP.Color.V = ColorName.LightRed;

            MP = mp; MP.Stat.V = nameof(MP);
            MP.EnableDragging = false;
            MP.Color.V = ColorName.LightBlue;

            XP = xp; XP.Stat.V = nameof(XP);
            XP.EnableDragging = false;
            XP.Color.V = ColorName.LightYellow;

            QuickBar = quickbar;
            QuickBar.EnableDragging = false;

            EquipmentDisplay = equipmentDisplay;
            EquipmentDisplay.EnableDragging = false;

            Viewport = new Viewport(ui.Input, MiniMap.FloorSystem, MiniMap.FactionSystem, res, loop, ui.Store);
            Viewport.Background.V = Color.Black;

            ActionSystem = act; // TODO: Make CurrentTurn a singleton dependency?
            LogBox.EnableDragging = false;
            MiniMap.EnableDragging = false;

            CoreData.View.WindowSize.ValueChanged += (e) =>
            {
                if (Layout is null) return;
                Layout.Size.V = e.NewValue;
            };
        }
        protected override void DefaultSize()
        {
            Layout.Size.V = UI.Window.Size;
        }

        public override void Close(ModalWindowButton buttonPressed)
        {
            base.Close(buttonPressed);
            OnActorDeselected();
        }

        public void OnActorDeselected()
        {
            MiniMap.Following.V = null;
            LogBox.Following.V = null;
            Viewport.Following.V = null;
            EquipmentDisplay.Following.V = null;
            Viewport.SetDirty();
        }

        public override void Update(TimeSpan t, TimeSpan dt)
        {
            base.Update(t, dt);
#if DEBUG
            if (UI.Input.IsButtonPressed(SFML.Window.Mouse.Button.Right))
            {
                var options = Layout.HitTest(UI.Input.GetMousePosition())
                    .Select(x => $"{x.GetType()} {x}")
                    .ToArray();
                UI.NecessaryChoice(options);
                foreach (var wnd in UI.GetOpenModals().ToList())
                {
                    options = wnd.Layout.HitTest(UI.Input.GetMousePosition())
                        .Select(x => $"{x.GetType()} {x}")
                        .ToArray();
                    if (options.Any())
                        UI.NecessaryChoice(options);
                }
            }
#endif
        }

        public void OnPointSelected(Coord p)
        {
            // Viewport.Following.V = null;
            var viewSize = Viewport.ViewArea.V.Size();
            Viewport.ViewArea.V = new(p.X - viewSize.X / 2, p.Y - viewSize.Y / 2, viewSize.X, viewSize.Y);
            Viewport.SetDirty();
        }

        public void OnActorSelected(Actor a)
        {
            MiniMap.SetDirty();
            MiniMap.Following.V = a;
            LogBox.Following.V = a;
            Viewport.Following.V = a;
            EquipmentDisplay.Following.V = a;
            var pos = a.Position();
            var viewSize = Viewport.ViewArea.V.Size();
            Viewport.ViewArea.V = new(pos.X - viewSize.X / 2, pos.Y - viewSize.Y / 2, viewSize.X, viewSize.Y);
            Viewport.SetDirty();
        }

        public override void Draw(RenderTarget target, RenderStates states)
        {
            if (!IsOpen)
                return;
            if (Viewport.ToolTip.V is null)
                Viewport.ToolTip.V = new CellToolTip(UI);
            // TODO: use UI events
            if (Viewport.Following.V is { } following)
            {
                var floorId = following.FloorId();
                var position = following.Position();
                PlayerNameLabel.Text.V = $"{following.Info.Name}, lv.{following.ActorProperties.Level.V} {following.ActorProperties.Race}";
                CurrentTurnLabel.Text.V = $"TURN {ActionSystem.CurrentTurn}";
                CurrentPlaceLabel.Text.V = $"{floorId.Branch} {floorId.Depth}";
                CurrentPosLabel.Text.V = $"X{position.X} Y{position.Y}";
                RngSeedLabel.Text.V = $"{Rng.GetGlobalSeed():x}";

                if (Viewport.MouseToWorldPos().TryGetValue(out var mouseOver))
                {
                    CurrentPosLabel.Text.V = $"X{mouseOver.X} Y{mouseOver.Y}";
                }

                if (following.ActorProperties.Health is { Min: _, Max: var maxHp, V: var hp })
                {
                    HP.Value.V = hp;
                    HP.MaxValue.V = maxHp;
                }
                if (following.ActorProperties.Magic is { Min: _, Max: var maxMp, V: var mp })
                {
                    MP.Value.V = mp;
                    MP.MaxValue.V = maxMp;
                }
                if (following.ActorProperties.Experience is { Min: _, Max: var maxXp, V: var xp })
                {
                    XP.Value.V = xp;
                    XP.MaxValue.V = maxXp;
                }
            }
            base.Draw(target, states);
        }

        protected override LayoutThemeBuilder DefineStyles(LayoutThemeBuilder builder) => base.DefineStyles(builder)
            .Rule<UIControl>(x => x
                .Apply(l =>
                {
                    if (l is not Header)
                        l.Background.V = UI.GetColor(ColorName.UIBackground);
                }))
            .Rule<Label>(x => x
                .Apply(l =>
                {
                    l.Padding.V = new(5, 0);
                }))
            .Rule<Label>(x => x
                .Match(x => x.HasClass("center"))
                .Apply(l => l.HorizontalAlignment.V = HorizontalAlignment.Center))
            .Rule<Label>(x => x
                .Match(x => x.HasClass("right"))
                .Apply(l => l.HorizontalAlignment.V = HorizontalAlignment.Right))
            ;

        public override LayoutGrid CreateLayout(LayoutGrid grid, string title)
        {
            var sidebarWidth = (UI.Store.Get(CoreData.View.WindowSize) - UI.Store.Get(Data.View.ViewportSize)).X;
            var logHeight = (UI.Store.Get(CoreData.View.WindowSize) - UI.Store.Get(Data.View.ViewportSize)).Y;

            return ApplyStyles(grid)
            .Row()
                .Col(id: "viewport")
                    .Row()
                        .Cell(Viewport)
                    .End()
                    .Row(h: 36, px: true, id: "quickbar")
                        .Cell<Layout>()
                        .Cell<UIWindowAsControl>(x => x.Window.V = QuickBar)
                    .End()
                .End()
                .Col(w: sidebarWidth, px: true, @class: "stat-panel")
                    .Row(h: 16, px: true, id: "name", @class: "center")
                        .Cell<Header>(x => PlayerNameLabel = x)
                    .End()
                    .Row(h: 4, px: true, @class: "spacer")
                        .Cell<Layout>()
                    .End()
                    .Row(h: 16, px: true, id: "hp-bar")
                        .Cell<Layout>()
                        .Cell<UIWindowAsControl>(x => x.Window.V = HP)
                    .End()
                    .Row(h: 4, px: true, @class: "spacer")
                        .Cell<Layout>()
                    .End()
                    .Row(h: 16, px: true, id: "mp-bar")
                        .Cell<Layout>()
                        .Cell<UIWindowAsControl>(x => x.Window.V = MP)
                    .End()
                    .Row(h: 4, px: true, @class: "spacer")
                        .Cell<Layout>()
                    .End()
                    .Row(h: 16, px: true, id: "xp-bar")
                        .Cell<Layout>()
                        .Cell<UIWindowAsControl>(x => x.Window.V = XP)
                    .End()
                    .Row(@class: "spacer")
                        .Cell<Layout>()
                    .End()
                    .Row(h: 36 * 5 + 18 * 5, px: true)
                        .Cell<UIWindowAsControl>(x => x.Window.V = EquipmentDisplay)
                    .End()
                    .Row(@class: "spacer")
                        .Cell<Layout>()
                    .End()
                    .Row(h: 16, px: true)
                        .Col(id: "time", @class: "center")
                            .Cell<Layout>()
                            .Cell<Header>(x => CurrentTurnLabel = x)
                        .End()
                        .Col(id: "pos", @class: "center")
                            .Cell<Layout>()
                            .Cell<Header>(x => CurrentPosLabel = x)
                        .End()
                    .End()
                    .Row(h: sidebarWidth, px: true, id: "mini-map")
                        .Cell<Layout>()
                        .Cell<UIWindowAsControl>(x => x.Window.V = MiniMap)
                    .End()
                    .Row(h: 16, px: true)
                        .Col(id: "place", @class: "center")
                            .Cell<Layout>()
                            .Cell<Header>(x => CurrentPlaceLabel = x)
                        .End()
                        .Col(id: "seed", @class: "center")
                            .Cell<Layout>()
                            .Cell<Header>(x => RngSeedLabel = x)
                        .End()
                    .End()
                .End()
            .End()
            .Row(h: logHeight, px: true, id: "log-panel")
                .Cell<UIWindowAsControl>(x => x.Window.V = LogBox)
            .End();
        }
    }
}
