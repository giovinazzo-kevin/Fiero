﻿using Fiero.Business.BUS.Structures.UI.Widgets;
using Fiero.Core;

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

        protected Label PlayerNameLabel { get; private set; }
        protected Label PlayerDescLabel { get; private set; }
        protected Label CurrentTurnLabel { get; private set; }
        protected Label CurrentPlaceLabel { get; private set; }
        protected Label CurrentPosLabel { get; private set; }
        protected Label RngSeedLabel { get; private set; }

        protected readonly ActionSystem ActionSystem;

        public MainSceneWindow(
            GameUI ui,
            LogBox logs, MiniMap miniMap, StatBar hp, StatBar mp, StatBar xp,
            ActionSystem act, GameResources res, GameLoop loop)
            : base(ui)
        {
            LogBox = logs;
            MiniMap = miniMap;
            HP = hp; HP.Stat.V = nameof(HP);
            HP.EnableDragging = false;
            HP.Color.V = ColorName.Red;

            MP = mp; MP.Stat.V = nameof(MP);
            MP.EnableDragging = false;
            MP.Color.V = ColorName.Blue;

            XP = xp; XP.Stat.V = nameof(XP);
            XP.EnableDragging = false;
            XP.Color.V = ColorName.Yellow;


            Viewport = new Viewport(ui.Input, MiniMap.FloorSystem, MiniMap.FactionSystem, res, loop);
            ActionSystem = act; // TODO: Make CurrentTurn a singleton dependency?
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
            HP.Open(string.Empty);
            MP.Open(string.Empty);
            XP.Open(string.Empty);
        }

        public override void Draw()
        {
            if (!IsOpen)
                return;
            // TODO: use UI events
            if (Viewport.Following.V is { } following)
            {
                var floorId = following.FloorId();
                var position = following.Position();
                PlayerNameLabel.Text.V = following.Info.Name;
                PlayerDescLabel.Text.V = $"Level {following.ActorProperties.Level.V} {following.ActorProperties.Race}";
                CurrentTurnLabel.Text.V = $"TURN {ActionSystem.CurrentTurn}";
                CurrentPlaceLabel.Text.V = $"{floorId.Branch} {floorId.Depth}";
                CurrentPosLabel.Text.V = $"X{position.X} Y{position.Y}";
                RngSeedLabel.Text.V = $"{Rng.GetGlobalSeed():x}";

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

        const int UISize = 240;
        public override LayoutGrid CreateLayout(LayoutGrid grid, string title) => ApplyStyles(grid)
            .Row()
                .Col(id: "viewport")
                    .Cell(Viewport)
                .End()
                .Col(w: UISize, px: true, @class: "stat-panel")
                    .Row(h: 24, px: true, id: "name", @class: "center")
                        .Cell<Label>(x => PlayerNameLabel = x)
                    .End()
                    .Row(h: 24, px: true, id: "desc", @class: "center")
                        .Cell<Label>(x => PlayerDescLabel = x)
                    .End()
                    .Row(h: 16, px: true, id: "hp-bar")
                        .Cell<UIWindowAsControl>(x => x.Window.V = HP)
                    .End()
                    .Row(h: 2, px: true, @class: "spacer")
                        .Cell<Layout>()
                    .End()
                    .Row(h: 16, px: true, id: "mp-bar")
                        .Cell<UIWindowAsControl>(x => x.Window.V = MP)
                    .End()
                    .Row(h: 2, px: true, @class: "spacer")
                        .Cell<Layout>()
                    .End()
                    .Row(h: 16, px: true, id: "xp-bar")
                        .Cell<UIWindowAsControl>(x => x.Window.V = XP)
                    .End()
                    .Row(@class: "spacer")
                        .Cell<Layout>()
                    .End()
                    .Row(h: 8, px: true)
                        .Col(id: "time")
                            .Cell<Label>(x => CurrentTurnLabel = x)
                        .End()
                        .Col(id: "place", @class: "right")
                            .Cell<Label>(x => CurrentPlaceLabel = x)
                        .End()
                    .End()
                    .Row(h: UISize, px: true, id: "mini-map")
                        .Cell<Layout>()
                        .Cell<UIWindowAsControl>(x => x.Window.V = MiniMap)
                    .End()
                    .Row(h: 8, px: true)
                        .Col(id: "pos")
                            .Cell<Label>(x => CurrentPosLabel = x)
                        .End()
                        .Col(id: "seed", @class: "right")
                            .Cell<Label>(x => RngSeedLabel = x)
                        .End()
                    .End()
                .End()
            .End()
            .Row(h: UISize, px: true, id: "log-panel")
                .Cell<UIWindowAsControl>(x => x.Window.V = LogBox)
            .End();
    }
}