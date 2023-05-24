using Fiero.Core;
using SFML.Graphics;
using System;
using System.Linq;
using Unconcern.Common;
using static Fiero.Business.Data;

namespace Fiero.Business
{
    public partial class RenderSystem : EcsSystem
    {
        protected readonly GameUI UI;
        protected readonly GameLoop Loop;
        protected readonly GameResources Resources;

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
            Window.Size.ValueChanged += (p, old) =>
            {
                DeveloperConsole.Size.V = Window.Size.V;
            };
        }

        public void Reset()
        {
            UI.Open(Window);
            UI.Open(DeveloperConsole);
            DeveloperConsole.Hide();
        }

        public void Update()
        {

            if (UI.Input.IsKeyPressed(UI.Store.Get(Hotkeys.DeveloperConsole)))
            {
                if (DeveloperConsole.Layout.IsHidden)
                    DeveloperConsole.Show();
                else
                    DeveloperConsole.Hide();
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

    }
}
