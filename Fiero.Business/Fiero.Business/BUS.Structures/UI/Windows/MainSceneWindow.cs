using Fiero.Core;
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

        protected Label PlayerNameLabel { get; private set; }
        protected Label PlayerDescLabel { get; private set; }
        protected Label CurrentTurnLabel { get; private set; }
        protected Label CurrentPlaceLabel { get; private set; }

        protected readonly ActionSystem ActionSystem;

        public MainSceneWindow(GameUI ui, LogBox logs, MiniMap miniMap, GameResources res, FactionSystem fac, DungeonSystem floor, ActionSystem act, GameLoop loop)
            : base(ui)
        {
            LogBox = logs;
            MiniMap = miniMap;
            Viewport = new Viewport(ui.Input, floor, fac, res, loop);
            ActionSystem = act; // TODO: Make CurrentTurn a singleton dependency
            LogBox.EnableDragging = false;
            MiniMap.EnableDragging = false;

            Data.UI.WindowSize.ValueChanged += (e) =>
            {
                Size.V = e.NewValue;
            };
        }

        public void OnActorDeselected()
        {
            MiniMap.Following.V = null;
            LogBox.Following.V = null;
            Viewport.Following.V = null;
            Viewport.SetDirty();
        }

        public void OnPointSelected(Coord p)
        {
            Viewport.Following.V = null;
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
            var pos = a.Position();
            var viewSize = Viewport.ViewArea.V.Size();
            Viewport.ViewArea.V = new(pos.X - viewSize.X / 2, pos.Y - viewSize.Y / 2, viewSize.X, viewSize.Y);
            Viewport.SetDirty();
        }

        public override void Open(string title)
        {
            base.Open(title);
            MiniMap.Open(string.Empty);
            LogBox.Open(string.Empty);
        }

        public override void Draw()
        {
            if (!IsOpen)
                return;
            if (Viewport.Following.V != null)
            {
                var floorId = Viewport.Following.V.FloorId();
                PlayerNameLabel.Text.V = Viewport.Following.V.Info.Name;
                PlayerDescLabel.Text.V = Viewport.Following.V.ActorProperties.Type.ToString();
                CurrentTurnLabel.Text.V = $"TURN {ActionSystem.CurrentTurn}";
                CurrentPlaceLabel.Text.V = $"{floorId.Branch} {floorId.Depth}";
            }
            base.Draw();
        }

        protected override LayoutStyleBuilder DefineStyles(LayoutStyleBuilder builder) => base.DefineStyles(builder)
            .AddRule<Label>(x => x
                .Apply(l =>
                {
                    l.Background.V = UI.GetColor(ColorName.UIBackground);
                    l.Padding.V = new(5, 0);
                }))
            .AddRule<Label>(x => x
                .Match(x => x.HasClass("center"))
                .Apply(l => l.HorizontalAlignment.V = HorizontalAlignment.Center))
            .AddRule<Label>(x => x
                .Match(x => x.HasClass("right"))
                .Apply(l => l.HorizontalAlignment.V = HorizontalAlignment.Right))
            ;

        public override LayoutGrid CreateLayout(LayoutGrid grid, string title) => ApplyStyles(grid)
            .Row()
                .Col(id: "viewport")
                    .Cell(Viewport)
                .End()
                .Col(w: 200, px: true, @class: "stat-panel")
                    .Row(h: 24, px: true, id: "name", @class: "center")
                        .Cell<Label>(x => PlayerNameLabel = x)
                    .End()
                    .Row(h: 24, px: true, id: "hp-bar")
                        .Cell<Layout>(x => x.Background.V = Color.Red)
                    .End()
                    .Row(h: 24, px: true, id: "mp-bar")
                        .Cell<Layout>(x => x.Background.V = Color.Blue)
                    .End()
                    .Row(h: 24, px: true, id: "xp-bar")
                        .Cell<Layout>(x => x.Background.V = Color.Yellow)
                    .End()
                    .Row(h: 24, px: true, id: "desc", @class: "center")
                        .Cell<Label>(x => PlayerDescLabel = x)
                    .End()
                    .Row(@class: "spacer")
                        .Cell<Layout>()
                    .End()
                    .Row(h: 24, px: true)
                        .Col(id: "time")
                            .Cell<Label>(x => CurrentTurnLabel = x)
                        .End()
                        .Col(id: "place", @class: "right")
                            .Cell<Label>(x => CurrentPlaceLabel = x)
                        .End()
                    .End()
                    .Row(h: 200, px: true, id: "mini-map")
                        .Cell<UIWindowAsControl>(x => x.Window.V = MiniMap)
                    .End()
                .End()
            .End()
            .Row(h: 200, px: true, id: "log-panel")
                .Cell<UIWindowAsControl>(x => x.Window.V = LogBox)
            .End();
    }
}
