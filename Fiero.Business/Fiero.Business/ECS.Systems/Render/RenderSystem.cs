using Fiero.Core;
using Fiero.Core.Structures;
using SFML.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unconcern.Common;
using static Fiero.Business.Data;

namespace Fiero.Business
{
    public partial class RenderSystem : EcsSystem
    {
        protected readonly GameUI UI;
        protected readonly GameLoop Loop;
        protected readonly GameResources Resources;
        protected readonly ConcurrentDictionary<int, ConcurrentQueue<OrderedPair<Coord, SpriteDef>>> Vfx = new();

        public readonly Viewport Viewport;
        public readonly MainSceneWindow Window;
        public readonly DeveloperConsole DeveloperConsole;

        public readonly SystemRequest<RenderSystem, PointSelectedEvent, EventResult> PointSelected;
        public readonly SystemRequest<RenderSystem, ActorSelectedEvent, EventResult> ActorSelected;
        public readonly SystemRequest<RenderSystem, ActorDeselectedEvent, EventResult> ActorDeselected;

        public void CenterOn(Actor a)
        {
            if (a.IsInvalid())
            {
                ActorDeselected.Handle(new());
            }
            else
            {
                ActorSelected.Handle(new(a));
            }
        }

        public void CenterOn(Coord c)
        {
            PointSelected.Handle(new(c));
        }

        public RenderSystem(EventBus bus, GameUI ui, GameLoop loop, GameResources resources, MainSceneWindow window, DeveloperConsole console) : base(bus)
        {
            UI = ui;
            Loop = loop;
            Resources = resources;
            Viewport = window.Viewport;
            Window = window;
            DeveloperConsole = console;
            ActorSelected = new(this, nameof(ActorSelected));
            PointSelected = new(this, nameof(PointSelected));
            ActorDeselected = new(this, nameof(ActorDeselected));
            PointSelected.ResponseReceived += (req, evt, res) =>
            {
                if (res.All(x => x))
                    Window.OnPointSelected(evt.Point);
            };
            ActorSelected.ResponseReceived += (req, evt, res) =>
            {
                if (res.All(x => x))
                    Window.OnActorSelected(evt.Actor);
            };
            ActorDeselected.ResponseReceived += (req, evt, res) =>
            {
                if (res.All(x => x))
                    Window.OnActorDeselected();
            };
        }

        public void Update()
        {
            if (!Window.IsOpen)
                UI.Show(Window);

            if (UI.Input.IsKeyPressed(UI.Store.Get(Hotkeys.DeveloperConsole)))
            {
                if (!DeveloperConsole.IsOpen)
                    UI.Show(DeveloperConsole);
                else
                    DeveloperConsole.Close(ModalWindowButton.None);
            }
        }

        public void Draw()
        {
            DrawVfx();
        }

        protected void DrawVfx()
        {
            var viewPos = Viewport.ViewArea.V.Position();
            foreach (var k in Vfx.Keys)
            {
                if (!Vfx.TryGetValue(k, out var anim))
                {
                    continue;
                }
                for (int j = 0, animCount = anim.Count; j < animCount && anim.TryDequeue(out var pair); j++)
                {
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

        public FloorId GetViewportFloor() => Viewport.Following.V?.FloorId() ?? default;
        public Coord GetViewportTileSize() => Viewport.ViewTileSize.V;
        public Coord GetViewportPosition() => Viewport.ViewArea.V.Position();
        public Coord GetViewportCenter() => Viewport.ViewArea.V.Position() + Viewport.ViewArea.V.Size() / 2;
        public IntRect GetViewportArea() => Viewport.ViewArea.V;

        public void ShowTargetingShape(TargetingShape shape) => Viewport.TargetingShape.V = shape;
        public void HideTargetingShape() => Viewport.TargetingShape.V = null;
        public TargetingShape GetTargetingShape() => Viewport.TargetingShape.V ?? throw new InvalidOperationException();

        public void Animate(bool blocking, Coord worldPos, params Animation[] animations)
        {
            if (blocking)
            {
                Impl();
            }
            else
            {
                Task.Run(Impl);
            }
            void Impl()
            {
                var time = TimeSpan.Zero;
                var increment = TimeSpan.FromMilliseconds(4);
                var timeline = animations.SelectMany(Timeline)
                    .OrderBy(x => x.Time)
                    .ToList();
                var viewPos = Viewport.ViewArea.V.Position();
                var myVfx = new ConcurrentQueue<OrderedPair<Coord, SpriteDef>>();
                var k = Vfx.Keys.LastOrDefault() + 1;
                Vfx[k] = myVfx;
                while (timeline.Count > 0)
                {
                    for (int i = timeline.Count - 1; i >= 0; i--)
                    {
                        var t = timeline[i];
                        if (time <= t.Time + t.Frame.Duration && time > t.Time)
                        {
                            foreach (var spriteDef in t.Frame.Sprites)
                            {
                                myVfx.Enqueue(new(worldPos, spriteDef));
                            }
                            t.Anim.OnFramePlaying(t.Index);
                        }
                        else if (time > t.Time + t.Frame.Duration)
                        {
                            timeline.RemoveAt(i);
                        }
                    }
                    if (blocking)
                    {
                        Loop.WaitAndDraw(increment);
                    }
                    else
                    {
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
                for (int i = 0; i < anim.Frames.Length; ++i)
                {
                    yield return (i, anim, time, anim.Frames[i]);
                    time += anim.Frames[i].Duration;
                }
            }
        }
    }
}
