using Fiero.Core;
using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Unconcern.Common;

namespace Fiero.Business
{
    public partial class RenderSystem : EcsSystem
    {
        protected readonly GameUI UI;
        protected readonly GameLoop Loop;
        protected readonly GameResources Resources;

        protected Layout Layout { get; private set; }
        protected Viewport Viewport { get; private set; }
        protected Paragraph Logs { get; private set; }
        protected Label LookBar { get; private set; }

        protected readonly List<Sprite> Vfx = new();

        public FloorId GetViewportFloor() => Viewport.ViewFloor.V;
        public Coord GetViewportTileSize() => Viewport.ViewTileSize.V;
        public Coord GetViewportPosition() => Viewport.ViewArea.V.Position();
        public Coord GetViewportCenter() => Viewport.ViewArea.V.Position() + Viewport.ViewArea.V.Size() / 2;
        public IntRect GetViewportArea() => Viewport.ViewArea.V;

        public void CenterOn(Actor a)
        {
            var pos = a.Physics.Position;
            var viewSize = Viewport.ViewArea.V.Size();

            Viewport.ViewFloor.V = a.ActorProperties.FloorId;
            Viewport.ViewArea.V = new(pos.X - viewSize.X / 2, pos.Y - viewSize.Y / 2, viewSize.X, viewSize.Y);

            if(a.Log != null) {
                Logs.Text.V = String.Join('\n', a.Log.GetMessages().TakeLast(Logs.MaxLines));
            }

            Viewport.SetDirty();
        }

        public bool IsCursorVisible() => Viewport.CursorPosition.V.HasValue;
        public Coord GetCursorPosition() => Viewport.CursorPosition.V ?? throw new InvalidOperationException();

        public void ShowCursor(Coord worldPos, Color? c = null)
        {
            Viewport.CursorPosition.V = worldPos;
            Viewport.CursorColor.V = c ?? Color.White;
        }

        public void MoveCursor(Coord offset)
        {
            if(!Viewport.CursorPosition.V.HasValue) {
                throw new InvalidOperationException();
            }
            var viewArea = GetViewportArea();
            Viewport.CursorPosition.V = (Viewport.CursorPosition.V.Value + offset).Clamp(
                viewArea.Left, viewArea.Left + viewArea.Width - 1, 
                viewArea.Top, viewArea.Top + viewArea.Height - 1
            );
        }

        public void HideCursor()
        {
            Viewport.CursorPosition.V = null;
            Viewport.CursorColor.V = Color.White;
        }

        public void SetLookText(string text)
        {
            LookBar.Text.V = text;
        }

        public void Animate(Coord worldPos, params Animation[] animations)
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
                            var sprite = Resources.Sprites.Get(spriteDef.Texture, spriteDef.Sprite);
                            sprite.Position = (worldPos - viewPos + spriteDef.Offset) * Viewport.ViewTileSize.V + Viewport.Position.V;
                            sprite.Scale = Viewport.ViewTileSize.V / sprite.GetLocalBounds().Size();
                            sprite.Color = spriteDef.Tint;
                            Vfx.Add(sprite);
                        }
                        t.Anim.OnFramePlaying(t.Index);
                        // AnimationFramePlayed.Raise(new(t.Anim, t.Index));
                    }
                    else if (time >= t.Time + t.Frame.Duration) {
                        timeline.RemoveAt(i);
                    }
                }
                Loop.WaitAndDraw(increment);
                time += increment;
                foreach (var sprite in Vfx) {
                    sprite.Dispose();
                }
                Vfx.Clear();
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

        public RenderSystem(EventBus bus, GameUI ui, GameLoop loop, GameResources resources) : base(bus)
        {
            UI = ui;
            Loop = loop;
            Resources = resources;
        }

        protected virtual LayoutGrid BuildLayout(LayoutGrid grid) => grid
            .Row()
                .Row(h: 4 * 0.025f, id: "top-bar")
                    .Cell<Label>(x => x.Text.V = "Fiero")
                .End()
                .Row(h: 4 * 0.775f, id: "player-view")
                    .Col()
                        .Cell<Viewport>(x => Viewport = x)
                    .End()
                .End()
                .Row(h: 4 * 0.175f, id: "player-logs")
                    .Cell<Paragraph>(x => Logs = x)
                .End()
                .Row(h: 4 * 0.025f, id: "look-bar")
                    .Cell<Label>(x => LookBar = x)
                .End()
            .End();

        public void Initialize()
        {
            Layout = UI.CreateLayout().Build(new(), BuildLayout);
            Data.UI.WindowSize.ValueChanged += args => {
                var oldValue = Viewport.Size.V;
                Layout.Size.V = args.NewValue;
                var newValue = Viewport.Size.V;
                var recenter = (oldValue - newValue) / Viewport.ViewTileSize.V / 2;
                var viewPos = Viewport.ViewArea.V.Position() + recenter;
                var viewSize = newValue / Viewport.ViewTileSize.V;
                Viewport.ViewArea.V = new(viewPos.X, viewPos.Y, viewSize.X, viewSize.Y);
            };
            Viewport.ViewTileSize.ValueChanged += (e, oldValue) => {
                var newValue = Viewport.ViewTileSize.V;
                var viewSize = Viewport.ViewArea.V.Size();
                var recenter = (viewSize * newValue - viewSize * oldValue) / newValue / 2;
                var viewPos = Viewport.ViewArea.V.Position() + recenter;
                viewSize = Viewport.Size.V / Viewport.ViewTileSize.V;
                Viewport.ViewArea.V = new(viewPos.X, viewPos.Y, viewSize.X, viewSize.Y);
            };
        }

        public void Update()
        {
            Layout.Update();
            if(UI.Input.IsKeyPressed(UI.Store.Get(Data.Hotkeys.ToggleZoom))) {
                Viewport.ViewTileSize.V = Viewport.ViewTileSize.V == new Coord(8, 8)
                    ? new Coord(16, 16) : new Coord(8, 8);
            }
        }

        public void Draw()
        {
            UI.Window.Draw(Layout);
            foreach (var vfx in Vfx) {
                UI.Window.Draw(vfx);
            }
        }
    }
}
