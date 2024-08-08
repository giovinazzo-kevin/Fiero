using Fiero.Core.Ergo;
using SFML.Graphics;

namespace Fiero.Business.Scenes
{
    public class CharCreationScene : GameScene<CharCreationScene.SceneState>
    {
        public enum SceneState
        {
            [EntryState]
            Main,
            [ExitState]
            StartGame,
            [ExitState]
            QuitToMenu
        }

        public enum MenuOptions
        {
            StartGame,
            QuitToMenu
        }

        protected readonly GameUI UI;
        protected readonly GameResources Resources;
        protected readonly GameDataStore Store;
        protected readonly OffButton OffButton;

        protected UIControl Layout { get; private set; }

        public readonly LayoutRef<TextBox> PlayerName = new();
        public readonly LayoutRef<TextBox> Seed = new();


        public CharCreationScene(
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

        private string L(string K) => Resources.Localizations.Translate(K);

        private Picture selectedPicture;
        private LoadoutName selectedLoadout;

        void LoadoutIcon(Picture x, NpcName icon, LoadoutName loadoutName)
        {
            x.Sprite.V = Resources.Sprites.Get(TextureName.Creatures, icon.ToString(), ColorName.White);
            x.OutlineThickness.V = 2;
            x.OutlineColor.V = Color.Transparent;
            var tooltip = L($"$CharCreation.Loadout{loadoutName}$");
            x.MouseOver((_, __) =>
            {
                if (x != selectedPicture)
                    x.OutlineColor.V = Color.Yellow;
                ((SimpleToolTip)x.ToolTip)
                    .SetText(tooltip);
            }, (_, __) =>
            {
                if (x != selectedPicture)
                    x.OutlineColor.V = Color.Transparent;
            });
            x.IsActive.ValueChanged += (_, __) =>
            {
                if (x.IsActive)
                {
                    if (selectedPicture != null)
                        selectedPicture.OutlineColor.V = Color.Transparent;
                    selectedPicture = x;
                    selectedLoadout = loadoutName;
                    x.OutlineColor.V = Color.Red;
                }
                else
                {
                    if (x.IsMouseOver)
                    {
                        if (selectedPicture == x)
                        {
                            selectedLoadout = LoadoutName.None;
                            selectedPicture = null;
                        }
                        x.OutlineColor.V = Color.Yellow;
                    }
                }
            };
            x.ToolTip.V = new SimpleToolTip(UI);
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            if (!Resources.Scripts.Get<ErgoLayoutScript>("layout_char_creation")
                .TryCreateComponent("layout_char_creation", out var layout))
                throw new InvalidOperationException();
            Layout = UI.CreateLayout().Build(new(), layout);

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
                l.Text.V = L($"$CharCreation.{option}$");
                l.Clicked += (_, __, ___) =>
                {
                    TrySetState(state);
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
                case SceneState.StartGame:
                    _ = UI.Input.ForceRestoreFocus();
                    Store.SetValue(Data.Player.Name, PlayerName.Control.DisplayText);
                    Store.SetValue(Data.Player.Loadout, selectedLoadout);
                    if (!string.IsNullOrWhiteSpace(Seed.Control.Text.V))
                    {
                        var newRngSeed = Seed.Control.Text.V
                            .Select(x => (int)x)
                            .Aggregate((a, b) => a + b * 17);
                        Store.SetValue(Data.Global.RngSeed, newRngSeed);
                        Rng.SetGlobalSeed(newRngSeed);
                    }
                    break;
                case SceneState.QuitToMenu:
                    break;
            }
        }
    }
}
