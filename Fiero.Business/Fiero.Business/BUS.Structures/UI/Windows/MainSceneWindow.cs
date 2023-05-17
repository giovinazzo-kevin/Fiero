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

        public MainSceneWindow(GameUI ui, LogBox logs, MiniMap miniMap, GameResources res, FactionSystem fac, DungeonSystem floor)
            : base(ui)
        {
            LogBox = logs;
            MiniMap = miniMap;
            Viewport = new Viewport(ui.Input, floor, fac, res);

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

        protected override LayoutStyleBuilder DefineStyles(LayoutStyleBuilder builder) => base.DefineStyles(builder)
            ;

        public override LayoutGrid CreateLayout(LayoutGrid grid, string title) => grid
            .Row()
                .Col(id: "viewport")
                    .Cell(Viewport)
                .End()
                .Col(w: 200, px: true, @class: "stat-panel")
                    .Row(h: 32, px: true, id: "hp-bar")
                        .Cell<Layout>(x => x.Background.V = Color.Red)
                    .End()
                    .Row(h: 32, px: true, id: "mp-bar")
                        .Cell<Layout>(x => x.Background.V = Color.Blue)
                    .End()
                    .Row()
                        .Cell<Layout>(x => x.Background.V = Color.Green)
                    .End()
                    .Row(h: 200, px: true, id: "mini-map")
                        .Cell<UIWindowControl>(x => x.Window.V = MiniMap)
                    .End()
                .End()
            .End()
            .Row(h: 200, px: true, id: "log-panel")
                .Cell<UIWindowControl>(x => x.Window.V = LogBox)
            .End();
    }
}
