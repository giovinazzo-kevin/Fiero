using Fiero.Core;
using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using Unconcern.Common;

namespace Fiero.Business
{
    public partial class RenderSystem : EcsSystem
    {
        protected readonly GameUI UI;
        protected readonly GameLoop Loop;
        protected readonly GameResources Resources;

        public UIScreen Screen { get; private set; }

        public RenderSystem(EventBus bus, GameUI ui, GameLoop loop, GameResources resources) : base(bus)
        {
            UI = ui;
            Loop = loop;
            Resources = resources;
        }

        public void Initialize()
        {
            Screen = new UIScreen(UI, Resources, Loop);
            Screen.Open("Game", Array.Empty<ModalWindowButton>(), ModalWindowStyles.None);
        }

        public void Update()
        {
            Screen.Update();
        }

        public void Draw()
        {
            Screen.Draw();
        }
    }
}
