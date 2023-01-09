using Fiero.Core;
using System.Linq;
using Unconcern.Common;

namespace Fiero.Business
{
    public partial class InterfaceSystem : EcsSystem
    {
        protected readonly GameUI UI;
        protected readonly GameLoop Loop;
        protected readonly GameResources Resources;
        // TODO: Refactor away coupling to systems caused by Minimap
        protected readonly DungeonSystem DungeonSystem;
        protected readonly FactionSystem FactionSystem;
        protected readonly GameColors<ColorName> Colors;
        protected Layout Layout { get; private set; }

        public StatBar HPBar { get; private set; }
        public StatBar MPBar { get; private set; }
        public Minimap Minimap { get; private set; }

        public readonly SystemRequest<InterfaceSystem, ActorSelectedEvent, EventResult> ActorSelected;
        public readonly SystemRequest<InterfaceSystem, ActorDeselectedEvent, EventResult> ActorDeselected;

        public InterfaceSystem(EventBus bus, GameUI ui, DungeonSystem dungeonSystem, FactionSystem factionSystem, GameLoop loop, GameResources resources, GameColors<ColorName> colors) : base(bus)
        {
            UI = ui;
            Loop = loop;
            Resources = resources;
            Colors = colors;
            DungeonSystem = dungeonSystem;
            FactionSystem = factionSystem;

            ActorSelected = new(this, nameof(ActorSelected));
            ActorDeselected = new(this, nameof(ActorDeselected));

            ActorSelected.ResponseReceived += (req, evt, res) =>
            {
                if (res.All(x => x))
                {
                    Minimap.SetDirty();
                    Minimap.Following.V = evt.Actor;
                    HPBar.Value.V = Minimap.Following.V.ActorProperties.Health.V;
                    HPBar.MaxValue.V = Minimap.Following.V.ActorProperties.Health.Max;
                    MPBar.Value.V = Minimap.Following.V.ActorProperties.Health.V;
                    MPBar.MaxValue.V = Minimap.Following.V.ActorProperties.Health.Max;
                }
            };

            ActorDeselected.ResponseReceived += (req, evt, res) =>
            {
                if (res.All(x => x))
                {
                    Minimap.Following.V = null;
                }
            };
        }

        public void Reset()
        {
            UI.Show(HPBar);
            UI.Show(MPBar);
            UI.Show(Minimap);
            Invalidate(UI.Store.Get(Data.UI.WindowSize));
        }

        protected virtual void Invalidate(Coord newSize)
        {
            var barSize = new Coord(200, 100);
            var mapSize = new Coord(256, 256);
            if (HPBar.Layout != null)
            {
                HPBar.Layout.Size.V = barSize;
                HPBar.Layout.Position.V = new(0, 0);
            }
            if (MPBar.Layout != null)
            {
                MPBar.Layout.Position.V = new(0, (int)(UI.Store.Get(Data.UI.TileSize) * 1.5));
                MPBar.Layout.Size.V = barSize;
            }
            if (Minimap.Layout != null)
            {
                Minimap.Layout.Position.V = new(newSize.X - mapSize.X, 0);
                Minimap.Layout.Size.V = mapSize;
            }

            Layout.Position.V = new();
            Layout.Size.V = newSize;
        }

        public void Initialize()
        {
            Layout = UI.CreateLayout().Build(new(), grid => grid
                .Row(w: 0.5f, h: 0.5f)
                .End()
            );
            HPBar = new(UI, "HP", ColorName.LightRed);
            MPBar = new(UI, "MP", ColorName.LightBlue);
            Minimap = new(UI, DungeonSystem, FactionSystem, Colors);
            Data.UI.WindowSize.ValueChanged += (args) =>
            {
                Invalidate(args.NewValue);
            };
        }

        public void Update()
        {
            Layout.Update();
        }

        public void Draw()
        {
            UI.Window.RenderWindow.Draw(Layout);
        }
    }
}
