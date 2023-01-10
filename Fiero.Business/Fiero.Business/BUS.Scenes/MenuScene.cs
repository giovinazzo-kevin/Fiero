using Fiero.Core;
using SFML.Graphics;
using System;
using System.Threading.Tasks;

namespace Fiero.Business.Scenes
{
    public class MenuScene : GameScene<MenuScene.SceneState>
    {
        public enum SceneState
        {
            [EntryState]
            Main,
            [ExitState]
            Exit_NewGame,
            [ExitState]
            Exit_QuitGame
        }

        public enum MenuOptions
        {
            NewGame,
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
            Layout = UI.CreateLayout()
                .Build(UI.Store.Get(Data.UI.PopUpSize), grid => grid
                    .Style<Label>(s => s
                        .Match(x => x.HasClass("ng"))
                        .Apply(l => { l.FontSize.V = 48; l.Foreground.V = Color.Yellow; }))
                    .Style<Label>(s => s
                        .Match(x => !x.HasClass("ng"))
                        .Apply(l => { l.FontSize.V = 24; }))
                    .Col()
                        .Row(h: 2, @class: "ng")
                            .Cell(MakeMenuButton(MenuOptions.NewGame, SceneState.Exit_NewGame))
                        .End()
                        .Row(h: 0.66f)
                            .Cell(MakeMenuButton(MenuOptions.Settings, SceneState.Exit_QuitGame))
                        .End()
                        .Row(h: 0.66f)
                            .Cell(MakeMenuButton(MenuOptions.QuitGame, SceneState.Exit_QuitGame))
                        .End()
                    .End()
                );

            Data.UI.WindowSize.ValueChanged += e =>
            {
                if (State == SceneState.Main)
                {
                    Layout.Position.V = e.NewValue / 4;
                    Layout.Size.V = e.NewValue / 2;
                }
            };

            Action<Button> MakeMenuButton(MenuOptions option, SceneState state) => l =>
            {
                l.Text.V = Resources.Localizations.Get($"Menu.{option}");
                l.Clicked += (_, __, ___) =>
                {
                    TrySetState(state);
                    return true;
                };
            };
        }

        public override void Update()
        {
            Layout.Update();
        }

        public override void Draw()
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
                // Initialize
                case SceneState.Main:
                    break;
                // New game
                case SceneState.Exit_NewGame:
                    Store.TrySetValue(Data.Player.Name, "Player", "Player");
                    break;
                // Quit game
                case SceneState.Exit_QuitGame:
                    // TODO: Are you sure?
                    OffButton.Press();
                    break;
            }
        }
    }
}
