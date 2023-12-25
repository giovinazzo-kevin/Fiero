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
            Exit_Continue,
            [ExitState]
            Exit_NewGame,
            [ExitState]
            Exit_LoadGame,
            [ExitState]
            Exit_QuitGame
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
            Layout = UI.CreateLayout()
                .Build(new(), grid => grid
                    .Style<Button>(s => s
                        .Match(x => x.HasClass("ng"))
                        .Apply(l => { l.FontSize.V = l.Font.V.Size * 2; l.Foreground.V = Color.Yellow; }))
                    .Style<Button>(s => s
                        .Match(x => !x.HasClass("ng"))
                        .Apply(l => { l.FontSize.V = l.Font.V.Size; }))
                    .Style<Button>(s => s
                        .Apply(l => { l.HorizontalAlignment.V = HorizontalAlignment.Center; }))
                    .Col()
                        .Row()
                            .Col()
                                .Cell(MakeMenuButton(MenuOptions.Continue, SceneState.Exit_Continue))
                            .End()
                            .Col(@class: "ng")
                                .Cell(MakeMenuButton(MenuOptions.NewGame, SceneState.Exit_NewGame))
                            .End()
                            .Col()
                                .Cell(MakeMenuButton(MenuOptions.LoadGame, SceneState.Exit_LoadGame))
                            .End()
                        .End()
                        .Row()
                            .Cell(MakeMenuButton(MenuOptions.Settings, SceneState.Exit_QuitGame))
                        .End()
                        .Row()
                            .Cell(MakeMenuButton(MenuOptions.QuitGame, SceneState.Exit_QuitGame))
                        .End()
                    .End()
                );

            Data.View.WindowSize.ValueChanged += e =>
            {
                if (State == SceneState.Main)
                {
                    Layout.Position.V = e.NewValue / 4;
                    Layout.Size.V = e.NewValue / 2;
                }
            };

            Action<Button> MakeMenuButton(MenuOptions option, SceneState state) => l =>
            {
                l.Text.V = Resources.Localizations.Translate($"$Menu.{option}$");
                l.Clicked += (_, __, ___) =>
                {
                    TrySetState(state);
                    return true;
                };
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
                case SceneState.Exit_NewGame:
                    Store.TrySetValue(Data.Player.Name, "Player", "Player");
                    break;
                case SceneState.Exit_LoadGame:
                    // TODO: Save/load
                    break;
                case SceneState.Exit_QuitGame:
                    // TODO: Are you sure?
                    OffButton.Press();
                    break;
            }
        }
    }
}
