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
        protected readonly ConcurrentQueue<OrderedPair<Coord, AnimationSprite>> Vfx = new();
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
            PlayerHP.Progress.V = a.ActorProperties.Stats.HealthPercentage;
            PlayerHPLabel.Text.V = $"HP: {a.ActorProperties.Stats.Health}/{a.ActorProperties.Stats.MaximumHealth}";
            PlayerMP.Progress.V = a.ActorProperties.Stats.HealthPercentage;
            PlayerMPLabel.Text.V = $"MP: {a.ActorProperties.Stats.Health}/{a.ActorProperties.Stats.MaximumHealth}";

            PlayerStats.Text.V = $"Floor: {a.FloorId()}";

            Minimap.SetDirty();
            Viewport.SetDirty();
        }

        public void CenterOn(Coord pos)
        {
            var viewSize = Viewport.ViewArea.V.Size();
            Viewport.ViewArea.V = new(pos.X - viewSize.X / 2, pos.Y - viewSize.Y / 2, viewSize.X, viewSize.Y);
            Viewport.SetDirty();
        }

        public void SetLookText(string text)
        {
            Look.Text.V = text;
        }


        public void Animate(bool blocking, Coord worldPos, params Animation[] animations)
        {
            _ = Impl();
            async Task Impl()
            {
                var time = TimeSpan.Zero;
                var increment = TimeSpan.FromMilliseconds(10);
                var timeline = animations.SelectMany(a => Timeline(a))
                    .OrderBy(x => x.Time)
                    .ToList();
                var viewPos = Viewport.ViewArea.V.Position();
                while (timeline.Count > 0) {
                    for (int i = timeline.Count - 1; i >= 0; i--) {
                        var t = timeline[i];
                        if (time < t.Time + t.Frame.Duration && time >= t.Time) {
                            foreach (var spriteDef in t.Frame.Sprites) {
                                Vfx.Enqueue(new(worldPos, spriteDef));
                            }
                            t.Anim.OnFramePlaying(t.Index);
                            // AnimationFramePlayed.Raise(new(t.Anim, t.Index));
                        }
                        else if (time >= t.Time + t.Frame.Duration) {
                            timeline.RemoveAt(i);
                        }
                    }
                    if (blocking) {
                        Loop.WaitAndDraw(increment);
                    }
                    else {
                        await Task.Delay(increment);
                    }
                    time += increment;
                    Vfx.Clear();
                }
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
                        .Row(h: 3 * 0.75f, id: "viewport")
                            .Cell<Viewport>(x => Viewport = x)
                        .End()
                        .Row(h: 3 * 0.025f, id: "look-bar")
                            .Cell<Label>(x => Look = x)
                        .End()
                        .Row(h: 3 * 0.225f, id: "logs")
                            .Cell<Paragraph>(x => Logs = x)
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
            if (UI.Input.IsKeyPressed(UI.Store.Get(Data.Hotkeys.ToggleZoom))) {
                Viewport.ViewTileSize.V = Viewport.ViewTileSize.V == new Coord(16, 16)
                    ? new Coord(32, 32) : new Coord(16, 16);
            }
        }

        public override void Draw()
        {
            base.Draw();
            var viewPos = Viewport.ViewArea.V.Position();

            var vfxCount = Vfx.Count;
            for(int i = 0; i < vfxCount; ++i)
            { 
                if(!Vfx.TryDequeue(out var pair)) {
                    break;
                }
                var (worldPos, spriteDef) = (pair.Left, pair.Right);
                using var sprite = new Sprite(Resources.Sprites.Get(spriteDef.Texture, spriteDef.Sprite));
                var spriteSize = sprite.GetLocalBounds().Size();
                sprite.Position = (worldPos - viewPos + spriteDef.Offset) * Viewport.ViewTileSize.V + Viewport.Position.V;
                sprite.Scale = Viewport.ViewTileSize.V / spriteSize * spriteDef.Scale;
                sprite.Color = Resources.Colors.Get(spriteDef.Tint);
                sprite.Origin = new Vec(0.5f, 0.5f) * spriteSize;
                UI.Window.Draw(sprite);
                Vfx.Enqueue(pair);
            }
        }

        protected override void OnWindowSizeChanged(GameDatumChangedEventArgs<Coord> args)
        {
            base.OnWindowSizeChanged(args);
            var oldValue = Viewport.Size.V;
            Layout.Size.V = args.NewValue;
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
