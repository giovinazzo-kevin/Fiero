using Fiero.Core;
using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fiero.Business.Scenes
{
    public class MenuScene : GameScene<MenuScene.SceneState>
    {
        public enum SceneState  
        {
            Main,
            Exit_NewGame,
            Exit_Tracker,
            Exit_QuitGame
        }

        public enum MenuOptions
        {
            NewGame,
            Settings,
            Tracker,
            QuitGame,
            
        }

        protected readonly GameUI UI;
        protected readonly GameLocalizations<LocaleName> Localizations;
        protected readonly GameDataStore Store;
        protected readonly OffButton OffButton;

        protected UIControl UI_Layout { get; private set; }
        protected Label UI_PlayerName { get; private set; }

        protected static MenuOptions[] AllOptions => Enum.GetValues<MenuOptions>();

        public MenuScene(GameDataStore store, GameUI ui, GameLocalizations<LocaleName> locals, OffButton off)
        {
            UI = ui;
            Localizations = locals;
            Store = store;
            OffButton = off;
        }

        public override void Initialize()
        {
            base.Initialize();
            UI_Layout = UI.CreateLayout()
                .Build(new(800, 800), grid => grid
                    .Style<Label>(l => { l.FontSize.V = 48; l.Foreground.V = Color.Yellow; }, 
                        match: g => g.Class == "ng")
                    .Style<Label>(l => { l.FontSize.V = 24; }, 
                        match: g => g.Class != "ng")
                    .Col()
                        .Row(h: 2, @class: "ng")
                            .Cell(MakeMenuButton(MenuOptions.NewGame, SceneState.Exit_NewGame))
                        .End()
                        .Row(h: 0.66f)
                            .Cell(MakeMenuButton(MenuOptions.Settings, SceneState.Exit_QuitGame))
                        .End()
                        .Row(h: 0.66f)
                            .Cell(MakeMenuButton(MenuOptions.Tracker, SceneState.Exit_Tracker))
                        .End()
                        .Row(h: 0.66f)
                            .Cell(MakeMenuButton(MenuOptions.QuitGame, SceneState.Exit_QuitGame))
                        .End()
                    .End()
                );
            Data.UI.WindowSize.ValueChanged += e => {
                UI_Layout.Position.V = e.NewValue / 4;
                UI_Layout.Size.V = e.NewValue / 2;
            };

            Action<Button> MakeMenuButton(MenuOptions option, SceneState state) => l => {
                l.Text.V = Localizations.Get($"Menu.{option}");
                l.Clicked += (_, __, ___) => TrySetState(state);
            };
        }

        public override void Update(RenderWindow win, float t, float dt)
        {
            UI_Layout.Update(t, dt);
        }

        public override void Draw(RenderWindow win, float t, float dt)
        {
            win.Clear(Color.Black);
            win.Draw(UI_Layout);
        }

        protected override bool CanChangeState(SceneState newState) => true;
        protected override void OnStateChanged(SceneState oldState)
        {
            switch(State) {
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
