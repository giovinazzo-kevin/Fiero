using Fiero.Core.Ergo;
using SFML.Graphics;

namespace Fiero.Business.Scenes
{
    public class MenuScene : GameScene<MenuScene.SceneState>
    {
        public enum SceneState
        {
            [EntryState]
            Main,
            [ExitState]
            Continue,
            [ExitState]
            NewGame,
            [ExitState]
            LoadGame,
            [ExitState]
            QuitGame
        }

        public enum MenuOptions
        {
            Continue,
            NewGame,
            LoadGame,
            Settings,
            QuitGame,
        }

        protected readonly GameUI UI;
        protected readonly GameResources Resources;
        protected readonly GameDataStore Store;
        protected readonly OffButton OffButton;

        protected UIControl Layout { get; private set; }

        public MenuScene(
            GameResources resources,
            GameDataStore store,
            GameUI ui,
            OffButton off
        )
        {
            UI = ui;
            Resources = resources;
            Store = store;
            OffButton = off;
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            if (!Resources.Scripts.Get<ErgoLayoutScript>("layout_menu").TryCreateComponent("Menu", out var menu))
                throw new InvalidOperationException();
            Layout = UI.CreateLayout().Build(new(), menu);

            Data.View.WindowSize.ValueChanged += e =>
            {
                if (State == SceneState.Main)
                {
                    Layout.Position.V = e.NewValue / 4;
                    Layout.Size.V = e.NewValue / 2;
                }
            };
        }

        public override void Update(TimeSpan t, TimeSpan dt)
        {
            Layout.Update(t, dt);
        }

        public override void DrawBackground(RenderTarget target, RenderStates states)
        {
            UI.Window.Clear(Color.Black);
            UI.Window.Draw(Layout);
        }

        protected override bool CanChangeState(SceneState newState) => true;
        protected override void OnStateChanged(SceneState oldState)
        {
            base.OnStateChanged(oldState);
            switch (State)
            {
                case SceneState.Main:
                    break;
                case SceneState.NewGame:
                    break;
                case SceneState.LoadGame:
                    // TODO: Save/load
                    break;
                case SceneState.QuitGame:
                    // TODO: Are you sure?
                    OffButton.Press();
                    break;
            }
        }
    }
}
