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
            Exit_QuitGame
        }

        public enum MenuOptions
        {
            NewGame,
            Settings,
            About,
            QuitGame,
            
        }

        protected readonly GameUI<FontName, TextureName, SoundName> UI;
        protected readonly GameLocalizations<LocaleName> Localizations;
        protected readonly GameDataStore Store;
        protected readonly OffButton OffButton;

        protected Layout UI_Layout { get; private set; }
        protected Label UI_PlayerName { get; private set; }

        protected static MenuOptions[] AllOptions => Enum.GetValues<MenuOptions>();

        public MenuScene(GameDataStore store, GameUI<FontName, TextureName, SoundName> ui, GameLocalizations<LocaleName> locals, OffButton off)
        {
            UI = ui;
            Localizations = locals;
            Store = store;
            OffButton = off;
        }

        public override void Initialize()
        {
            base.Initialize();
            var layoutBuilder = UI.CreateLayout()
                .WithFont(FontName.UI)
                .WithTexture(TextureName.UI)
                .WithTileSize(Store.GetOrDefault(Data.UI.TileSize, 8));

            layoutBuilder.Textbox(new(1, 1), 16, defaultText: "Player", 
                initialize: c => UI_PlayerName = c);
            layoutBuilder.Combobox<int>(new(18, 1), 16, initialize: control => control
                .AddOption("Warrior", 1)
                .AddOption("Rogue", 2)
                .AddOption("Wizard", 3));
            layoutBuilder.Button(new(1, 3), 16, Localizations.Get($"Menu.{MenuOptions.NewGame}"),
                initialize: control => control.Clicked += (_, __) => TrySetState(SceneState.Exit_NewGame));
            layoutBuilder.Button(new(1, 5), 16, Localizations.Get($"Menu.{MenuOptions.Settings}"));
            layoutBuilder.Button(new(1, 7), 16, Localizations.Get($"Menu.{MenuOptions.About}"));
            layoutBuilder.Button(new(1, 9), 16, Localizations.Get($"Menu.{MenuOptions.QuitGame}"), 
                initialize: control => control.Clicked += (_, __) => TrySetState(SceneState.Exit_QuitGame));
            layoutBuilder.ProgressBar(new(1, 16), 32, .55f);

            UI_Layout = layoutBuilder.Build();
        }

        public override void Update(RenderWindow win, float t, float dt)
        {
            UI_Layout.Update(t, dt);
        }

        public override void Draw(RenderWindow win, float t, float dt)
        {
            win.Clear();
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
                    Store.TrySetValue(Data.Player.Name, "Player", UI_PlayerName.Text);
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
