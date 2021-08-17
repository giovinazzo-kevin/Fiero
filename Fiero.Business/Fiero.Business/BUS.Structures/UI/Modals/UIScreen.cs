using Fiero.Core;
using SFML.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fiero.Business
{
    public class UIScreen : Modal
    {
        protected readonly GameLoop Loop;
        protected readonly ConcurrentDictionary<int, ConcurrentQueue<OrderedPair<Coord, SpriteDef>>> Vfx = new();
        protected Layout ViewportLayout { get; private set; }
        public Viewport Viewport { get; private set; }
        public Minimap Minimap { get; private set; }
        public Label Look { get; private set; }
        public Paragraph Logs { get; private set; }
        public Label PlayerName { get; private set; }
        public Label PlayerHPLabel { get; private set; }
        public ProgressBar PlayerHP { get; private set; }
        public Label PlayerMPLabel { get; private set; }
        public ProgressBar PlayerMP { get; private set; }
        public Paragraph PlayerStats { get; private set; }

        public UIScreen(GameUI ui, GameResources resources, GameLoop loop) : base(ui, resources)
        {
            Loop = loop;
            ViewportLayout = UI.CreateLayout().Build(new(), grid => grid
                .Row()
                    .Cell<Viewport>(x => {
                        Viewport = x;
                    })
                .End()
            );
        }

        public FloorId GetViewportFloor() => Viewport.Following.V?.FloorId() ?? default;
        public Coord GetViewportTileSize() => Viewport.ViewTileSize.V;
        public Coord GetViewportPosition() => Viewport.ViewArea.V.Position();
        public Coord GetViewportCenter() => Viewport.ViewArea.V.Position() + Viewport.ViewArea.V.Size() / 2;
        public IntRect GetViewportArea() => Viewport.ViewArea.V;

        public void ShowTargetingShape(TargetingShape shape) => Viewport.TargetingShape.V = shape;
        public void HideTargetingShape() => Viewport.TargetingShape.V = null;
        public TargetingShape GetTargetingShape() => Viewport.TargetingShape.V ?? throw new InvalidOperationException();

        public void CenterOn(Actor a)
        {
            var pos = a.Position();
            var viewSize = Viewport.ViewArea.V.Size();

            Minimap.Following.V = Viewport.Following.V = a;
            Viewport.ViewArea.V = new(pos.X - viewSize.X / 2, pos.Y - viewSize.Y / 2, viewSize.X, viewSize.Y);

            if (a.Log != null) {
                Logs.Text.V = String.Join('\n', a.Log.GetMessages().TakeLast(Logs.MaxLines));
            }

            PlayerName.Text.V = a.Info.Name;
            PlayerHP.Progress.V = a.ActorProperties.Health.Percentage;
            PlayerHPLabel.Text.V = $"HP: {a.ActorProperties.Health.V}/{a.ActorProperties.Health.Max}";
            PlayerMP.Progress.V = a.ActorProperties.Health.Percentage;
            PlayerMPLabel.Text.V = $"MP: {a.ActorProperties.Health.V}/{a.ActorProperties.Health.Max}";

            PlayerStats.Text.V = $"Floor: {a.FloorId()}";

            Minimap.SetDirty();
            Viewport.SetDirty();
        }

        public void CenterOn(Coord pos)
        {
            var viewSize = Viewport.ViewArea.V.Size();
            Viewport.ViewArea.V = new(pos.X - viewSize.X / 2, pos.Y - viewSize.Y / 2, viewSize.X, viewSize.Y);
            Minimap.SetDirty();
            Viewport.SetDirty();
        }

        public void SetDirty()
        {
            Minimap.SetDirty();
            Viewport.SetDirty();
        }

        public void SetLookText(string text)
        {
            Look.Text.V = text;
        }


        public void Animate(bool blocking, Coord worldPos, params Animation[] animations)
        {
            if(blocking) {
                Impl();
            }
            else {
                Task.Run(Impl);
            }
            void Impl()
            {
                var time = TimeSpan.Zero;
                var increment = TimeSpan.FromMilliseconds(4);
                var timeline = animations.SelectMany(a => Timeline(a))
                    .OrderBy(x => x.Time)
                    .ToList();
                var viewPos = Viewport.ViewArea.V.Position();
                var myVfx = new ConcurrentQueue<OrderedPair<Coord, SpriteDef>>();
                var k = Vfx.Keys.LastOrDefault() + 1;
                Vfx[k] = myVfx;
                while (timeline.Count > 0) {
                    for (int i = timeline.Count - 1; i >= 0; i--) {
                        var t = timeline[i];
                        if (time <= t.Time + t.Frame.Duration && time > t.Time) {
                            foreach (var spriteDef in t.Frame.Sprites) {
                                myVfx.Enqueue(new(worldPos, spriteDef));
                            }
                            t.Anim.OnFramePlaying(t.Index);
                        }
                        else if (time > t.Time + t.Frame.Duration) {
                            timeline.RemoveAt(i);
                        }
                    }
                    if (blocking) {
                        Loop.WaitAndDraw(increment);
                    }
                    else {
                        new GameLoop().Run(increment);
                    }
                    time += increment;
                    myVfx.Clear();
                }
                Vfx.Remove(k, out _);
            }

            IEnumerable<(int Index, Animation Anim, TimeSpan Time, AnimationFrame Frame)> Timeline(Animation anim)
            {
                var time = TimeSpan.Zero;
                for (int i = 0; i < anim.Frames.Length; ++i) {
                    yield return (i, anim, time, anim.Frames[i]);
                    time += anim.Frames[i].Duration;
                }
            }
        }

        protected override LayoutStyleBuilder DefineStyles(LayoutStyleBuilder builder)
        {
            return base.DefineStyles(builder)
                .AddRule<ProgressBar>(x => x
                    .Match(x => x.Id == "player-health-bar")
                    .Apply(x => x.Foreground.V = Resources.Colors.Get(ColorName.LightRed)))
                .AddRule<ProgressBar>(x => x
                    .Match(x => x.Id == "player-magic-bar")
                    .Apply(x => x.Foreground.V = Resources.Colors.Get(ColorName.LightBlue)))
                ;
        }

        protected override LayoutGrid RenderContent(LayoutGrid layout)
        {
            return base.RenderContent(layout)
                .Col(w: 2 * 0.75f)
                    .Row()
                        .Row(h: 3 * 0.85f, id: "viewport")
                            // This is empty space
                        .End()
                        .Row(h: 3 * 0.025f, id: "look-bar")
                            .Cell<Label>(x => Look = x)
                        .End()
                        .Row(h: 3 * 0.125f, id: "logs")
                            .Cell<Paragraph>(x => {
                                Logs = x;
                                Logs.MaxLines.V = 8;
                            })
                        .End()
                    .End()
                .End()
                .Col(w: 2 * 0.25f)
                    .Row(h: 5 * 0.025f, id: "player-name")
                        .Cell<Label>(x => PlayerName = x)
                    .End()
                    .Row(h: 5 * 0.125f)
                        .Row(id: "player-health-label")
                            .Cell<Label>(x => PlayerHPLabel = x)
                        .End()
                        .Row(id: "player-health-bar")
                            .Cell<ProgressBar>(x => PlayerHP = x)
                        .End()
                        .Row(id: "player-magic-label")
                            .Cell<Label>(x => PlayerMPLabel = x)
                        .End()
                        .Row(id: "player-magic-bar")
                            .Cell<ProgressBar>(x => PlayerMP = x)
                        .End()
                    .End()
                    .Row(h: 5 * 0.20f, id: "player-stats")
                        .Cell<Paragraph>()
                    .End()
                    .Row(h: 5 * 0.40f, id: "player-equipment")
                        .Cell<Paragraph>(x => PlayerStats = x)
                    .End()
                    .Row(h: 5 * 0.25f, id: "minimap")
                        .Cell<Minimap>(x => Minimap = x)
                    .End()
                .End()
                ;
        }

        public override void Update()
        {
            base.Update();
            ViewportLayout.Update();
            if (UI.Input.IsKeyPressed(UI.Store.Get(Data.Hotkeys.ToggleZoom))) {
                Viewport.ViewTileSize.V = Viewport.ViewTileSize.V == new Coord(32, 32)
                    ? new Coord(64, 64) : new Coord(32, 32);
            }
        }

        public override void Draw()
        {
            UI.Window.RenderWindow.Draw(ViewportLayout);
            base.Draw();
            var viewPos = Viewport.ViewArea.V.Position();
            foreach (var k in Vfx.Keys) {
                if(!Vfx.TryGetValue(k, out var anim)) {
                    continue;
                }
                for (int j = 0, animCount = anim.Count; j < animCount && anim.TryDequeue(out var pair); j++) {
                    var (worldPos, spriteDef) = (pair.Left, pair.Right);
                    using var sprite = new Sprite(Resources.Sprites.Get(spriteDef.Texture, spriteDef.Sprite, spriteDef.Color));
                    var spriteSize = sprite.GetLocalBounds().Size();
                    sprite.Position = (spriteDef.Offset + worldPos - viewPos) * Viewport.ViewTileSize.V + Viewport.Position.V;
                    sprite.Scale = Viewport.ViewTileSize.V / spriteSize * spriteDef.Scale;
                    sprite.Color = Resources.Colors.Get(spriteDef.Color);
                    sprite.Origin = new Vec(0.5f, 0.5f) * spriteSize;
                    UI.Window.Draw(sprite);
                    anim.Enqueue(pair);
                }
            }
        }

        protected override void OnWindowSizeChanged(GameDatumChangedEventArgs<Coord> args)
        {
            base.OnWindowSizeChanged(args);
            var oldValue = Viewport.Size.V;
            Layout.Size.V = args.NewValue;
            ViewportLayout.Size.V = args.NewValue;
            var newValue = Viewport.Size.V;
            var recenter = ((oldValue - newValue) / Viewport.ViewTileSize.V.ToVec() / 2).ToCoord();
            var viewPos = Viewport.ViewArea.V.Position() + recenter;
            var viewSize = newValue / Viewport.ViewTileSize.V;
            Viewport.ViewArea.V = new(viewPos.X, viewPos.Y, viewSize.X, viewSize.Y);
            PlayerMP.Center.V = PlayerHP.Center.V = true;
            PlayerMP.Length.V = PlayerHP.Length.V = Minimap.Size.V.X / PlayerHP.TileSize / 2 + 1;
            PlayerMP.Scale.V = PlayerHP.Scale.V = new(2, 2);
        }

        protected override void BeforePresentation()
        {
            base.BeforePresentation();
            Viewport.ViewTileSize.ValueChanged += ViewTileSize_ValueChanged;
            Closed += UIScreen_Closed;

            void UIScreen_Closed(ModalWindow arg1, ModalWindowButton arg2)
            {
                Viewport.ViewTileSize.ValueChanged -= ViewTileSize_ValueChanged;
                Closed -= UIScreen_Closed;
            }

            void ViewTileSize_ValueChanged(UIControlProperty<Coord> e, Coord oldValue)
            {
                var newValue = Viewport.ViewTileSize.V;
                var viewSize = Viewport.ViewArea.V.Size();
                var recenter = ((viewSize * newValue - viewSize * oldValue) / newValue.ToVec() / 2).ToCoord();
                var viewPos = Viewport.ViewArea.V.Position() + recenter;
                viewSize = Viewport.Size.V / Viewport.ViewTileSize.V;
                Viewport.ViewArea.V = new(viewPos.X, viewPos.Y, viewSize.X, viewSize.Y);
            }
        }
    }
}
