using SFML.Graphics;
using System.Collections.Concurrent;
using System.Diagnostics;
using Unconcern.Common;
using static Fiero.Business.Data;

namespace Fiero.Business
{
    public partial class RenderSystem : EcsSystem, Drawable
    {
        private volatile int _id;

        private readonly ConcurrentQueue<int> _toRemove = new();
        protected readonly Dictionary<int, Queue<OrderedPair<Coord, SpriteDef>>> Vfx = new();
        protected readonly Dictionary<int, Timeline> Timelines = new();

        protected readonly GameUI UI;
        protected readonly GameLoop Loop;
        protected readonly GameResources Resources;

        public readonly Viewport Viewport;
        public readonly MainSceneWindow Window;
        public readonly DeveloperConsole DeveloperConsole;

        public readonly SystemRequest<RenderSystem, PointSelectedEvent, EventResult> PointSelected;
        public readonly SystemRequest<RenderSystem, ActorSelectedEvent, EventResult> ActorSelected;
        public readonly SystemRequest<RenderSystem, ActorDeselectedEvent, EventResult> ActorDeselected;

        private readonly Stopwatch _sw = new();

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
            Viewport.Invalidate();
        }

        public void CenterOn(Coord c)
        {
            PointSelected.Handle(new(c));
            Viewport.Invalidate();
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
            _sw.Start();
        }

        public void Reset()
        {
            Window.Open("");
            UI.Open(DeveloperConsole);
            DeveloperConsole.Hide();
            Vfx.Clear();
            Timelines.Clear();
        }

        public void Update(TimeSpan t, TimeSpan dt)
        {
            Window.Update(t, dt);
            if (UI.Input.IsKeyPressed(UI.Store.Get(Hotkeys.DeveloperConsole)))
            {
                if (DeveloperConsole.Layout.IsHidden)
                    DeveloperConsole.Show();
                else
                    DeveloperConsole.Hide();
            }
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            Window.Draw(target, states);
            UpdateAnimations();
            foreach (var anim in Vfx.Values)
            {
                for (int j = 0, animCount = anim.Count; j < animCount && anim.TryDequeue(out var pair); j++)
                {
                    var (worldPos, spriteDef) = (pair.Left, pair.Right);
                    var screenPos = Viewport.WorldToScreenPos(worldPos);
                    using var sprite = new Sprite(Resources.Sprites.Get(spriteDef.Texture, spriteDef.Sprite, spriteDef.Color));
                    var spriteSize = sprite.GetLocalBounds().Size();
                    sprite.Position = Viewport.ViewTileSize.V * spriteDef.Offset + screenPos;
                    sprite.Scale = Viewport.ViewTileSize.V / spriteSize * spriteDef.Scale;
                    sprite.Origin = new Vec(0.5f, 0.5f) * spriteSize;
                    if (spriteDef.Alpha != 1)
                    {
                        var color = sprite.Color;
                        sprite.Color = Color.White;
                        target.Draw(sprite, states);
                        var alphaDt = (int)((1 - Math.Clamp(spriteDef.Alpha, 0, 1)) * 255);
                        sprite.Color = color.AddAlpha(-alphaDt);
                    }
                    target.Draw(sprite, states);
                    anim.Enqueue(pair);
                }
            }
        }

        void UpdateAnimations()
        {
            while (_toRemove.TryDequeue(out var id))
            {
                if (Timelines.TryGetValue(id, out var timeline))
                {
                    if (Vfx.TryGetValue(id, out var vfx))
                        vfx.Clear();
                    Vfx.Remove(id);
                    timeline.Animation.OnFramePlaying(timeline.Animation.Frames.Length);
                    timeline.Frames.Clear();
                    Timelines.Remove(id);
                }
            }
            var time = _sw.Elapsed;
            var keys = new List<int>(Timelines.Keys);
            foreach (var id in keys)
            {
                if (!Timelines.TryGetValue(id, out var timeline))
                    continue;
                var currentFrame = timeline.Frames.First();
                if (time >= currentFrame.End && Vfx.ContainsKey(id))
                {
                    Vfx[id].Clear();
                    Vfx.Remove(id);
                    timeline.Frames.RemoveAt(0);
                    if (!timeline.Frames.Any())
                    {
                        Timelines.Remove(id);
                        if (timeline.Animation.RepeatCount < 0 || timeline.Animation.RepeatCount > 0 && timeline.Animation.RepeatCount-- > 0)
                        {
                            Timelines[id] = new Timeline(timeline.Animation, timeline.ScreenPosition, _sw.Elapsed);
                        }
                        continue;
                    }
                }
                if (time > currentFrame.Start && !Vfx.ContainsKey(id))
                {
                    var myVfx = Vfx[id] = new();
                    foreach (var spriteDef in currentFrame.AnimFrame.Sprites)
                    {
                        myVfx.Enqueue(new(timeline.ScreenPosition, spriteDef));
                    }
                    timeline.Animation.OnFramePlaying(timeline.Animation.Frames.Length - timeline.Frames.Count);
                }
            }
        }

        public void StopAnimation(int id)
        {
            _toRemove.Enqueue(id);
        }

        public bool AlterAnimation(int id, Action<Animation> action)
        {
            if (Timelines.TryGetValue(id, out var timeline))
            {
                action(timeline.Animation);
                return true;
            }
            return false;
        }

        public int AnimateViewport(bool blocking, Coord worldPos, params Animation[] animations)
        {
            var t = _sw.Elapsed;
            var batch = animations.Select(a => new Timeline(a, worldPos, t)).ToList();
            foreach (var anim in batch)
            {
                if (!anim.Frames.Any())
                    continue;
                Timelines[Interlocked.Increment(ref _id)] = anim;
            }
            if (blocking)
            {
                Loop.WaitAndDraw(animations.Select(x => x.Duration).Max(), onUpdate: (t, ts) => UI.Input.Update());
            }
            return _id;
        }

        public FloorId GetViewportFloor() => Viewport.Following.V?.FloorId() ?? default;
        public Coord GetViewportTileSize() => Viewport.ViewTileSize.V;
        public Coord GetViewportPosition() => Viewport.ViewArea.V.Position();
        public Coord GetViewportCenter() => Viewport.ViewArea.V.Position() + Viewport.ViewArea.V.Size() / 2;
        public IntRect GetViewportArea() => Viewport.ViewArea.V;

        public void ShowTargetingShape(TargetingShape shape) => Viewport.TargetingShape.V = shape;
        public void HideTargetingShape() => Viewport.TargetingShape.V = null;
        public TargetingShape GetTargetingShape() => Viewport.TargetingShape.V ?? throw new InvalidOperationException();

    }
}
