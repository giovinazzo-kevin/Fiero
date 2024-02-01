using Fiero.Business.Scenes;
using SFML.Audio;
using SFML.Graphics;
using Unconcern.Common;

namespace Fiero.Business
{
    public class FieroGame : Game<FontName, TextureName, LocaleName, SoundName, ColorName, ShaderName, ScriptName>
    {
        protected readonly IEnumerable<IGameScene> Scenes;

        protected readonly EventBus Bus;
        protected readonly GameDataStore Store;
        protected readonly GameDialogues Dialogues;

        public FieroGame(
            EventBus bus,
            OffButton off,
            GameLoop loop,
            GameInput input,
            GameDirector director,
            GameDialogues dialogues,
            GameDataStore store,
            GameUI ui,
            GameWindow win,
            GameTextures<TextureName> textures,
            GameSprites<TextureName, ColorName> sprites,
            GameFonts<FontName> fonts,
            GameSounds<SoundName> sounds,
            GameScripts<ScriptName> scripts,
            GameColors<ColorName> colors,
            GameShaders<ShaderName> shaders,
            GameLocalizations<LocaleName> localization,
            IEnumerable<IGameScene> gameScenes,
            GameEntities entities,
            MetaSystem meta)
            : base(off, loop, input, textures, sprites, fonts, sounds, colors, shaders, scripts, localization, ui, win, director, entities, meta)
        {
            Bus = bus;
            Dialogues = dialogues;
            Scenes = gameScenes;
            Store = store;
            loop.TimeStep = TimeSpan.FromSeconds(1 / 144f);
            loop.MaxTimeStep = loop.TimeStep * 2;
            CreateGlobalTheme();
            Data.View.WindowSize.ValueChanged += WindowSize_ValueChanged;
            void WindowSize_ValueChanged(GameDatumChangedEventArgs<Coord> obj)
            {
                UI.Store.SetValue(Data.View.ViewportSize, obj.NewValue - new Coord(248, 128));
            }
        }

        protected void CreateGlobalTheme()
        {
            var P = 100;
            var transparentTypes = new[] { typeof(Label), typeof(Paragraph), typeof(Picture) };
            UI.Theme = new LayoutThemeBuilder()
                .Rule<UIControl>(b => b
                    .Apply(x => x.OutlineColor.V = UI.GetColor(ColorName.UIBorder))
                    .Apply(x => x.Background.V = UI.GetColor(ColorName.UIBackground))
                    .Apply(x => x.Foreground.V = UI.GetColor(ColorName.UIPrimary))
                    .Apply(x => x.Accent.V = UI.GetColor(ColorName.UIAccent))
                    .WithPriority(P))
                .Rule<UIControl>(b => b
                    .Filter(x => transparentTypes.Contains(x.GetType()))
                    .Apply(x => x.Background.V = UI.GetColor(ColorName.Transparent))
                    .WithPriority(P - 1))
                .Rule<Header>(b => b
                    .Apply(x => x.Background.V = UI.GetColor(ColorName.White))
                    .WithPriority(P - 1))
                .Rule<UIControl>(style => style
                    .Match(x => x.HasAllClasses("row-even"))
                    .Apply(x => x.Background.V = UI.GetColor(ColorName.UIBackground).AddRgb(8, 8, 8))
                    .WithPriority(P - 1))
                .Rule<UIControl>(style => style
                    .Match(x => x.HasAllClasses("row-odd"))
                    .Apply(x => x.Background.V = UI.GetColor(ColorName.UIBackground).AddRgb(-8, -8, -8))
                    .WithPriority(P - 1))
                .Rule<UIControl>(style => style
                    .Match(x => x.HasAllClasses("tooltip"))
                    .Apply(x => x.Background.V = UI.GetColor(ColorName.UIBackground).AddAlpha(-64))
                    .WithPriority(P - 1))

                .Build();
        }

        protected override void InitializeWindow(RenderWindow win)
        {
            base.InitializeWindow(win);
            win.Resized += (s, e) =>
            {
                var minSize = Store.Get(Data.View.MinWindowSize);
                var newSize = new Coord((int)e.Width, (int)e.Height).Clamp(minX: minSize.X, minY: minSize.Y);
                if (newSize != new Coord((int)e.Width, (int)e.Height))
                {
                    win.Size = newSize;
                }
                else
                {
                    Store.SetValue(Data.View.WindowSize, newSize);
                }
            };
            Store.SetValue(Data.View.WindowSize, win.Size.ToCoord());
            // This handler lets the window be resized by changing the WindowSize datum. Scripts can do this too.
            Data.View.WindowSize.ValueChanged += e =>
            {
                var size = win.Size.ToCoord();
                if (e.OldValue.Equals(size) && !e.NewValue.Equals(size))
                    win.Size = e.NewValue;
            };
        }

        protected override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            Store.RegisterByReflection(typeof(Data));

            Textures.CreateScratchTexture(new(16, 16));
            Textures.Add(TextureName.Creatures, new Texture("Resources/Textures/16x16_creatures.png"));
            Textures.Add(TextureName.Items, new Texture("Resources/Textures/16x16_items.png"));
            Textures.Add(TextureName.Features, new Texture("Resources/Textures/16x16_features.png"));
            Textures.Add(TextureName.Tiles, new Texture("Resources/Textures/16x16_tiles.png"));
            Textures.Add(TextureName.Spells, new Texture("Resources/Textures/16x16_spells.png"));
            Textures.Add(TextureName.Animations, new Texture("Resources/Textures/16x16_animations.png"));
            Textures.Add(TextureName.Icons, new Texture("Resources/Textures/16x16_icons.png"));
            Textures.Add(TextureName.UI, new Texture("Resources/Textures/8x8_ui.png"));
            Textures.Add(TextureName.FontMonospace, new Texture("Resources/Fonts/CGA8x8thick.png"));
            Textures.Add(TextureName.FontLight, new Texture("Resources/Fonts/Terminus_8x12.png"));
            Textures.Add(TextureName.FontTerminal, new Texture("Resources/Fonts/Terminus_8x12_syntax.png"));

            Fonts.Add(FontName.Monospace, new(8, 8), Textures.Get(TextureName.FontMonospace));
            Fonts.Add(FontName.Light, new(8, 12), Textures.Get(TextureName.FontLight));
            Fonts.Add(FontName.Terminal, new(8, 12), Textures.Get(TextureName.FontTerminal));


            //Shaders.Add(ShaderName.Test, new Shader(null, null, "Resources/Shaders/test.frag"));

            Sounds.Add(SoundName.Blip, new SoundBuffer("Resources/Sounds/speech/generic.wav"));
            Sounds.Add(SoundName.Ok, new SoundBuffer("Resources/Sounds/00_start1.wav"));
            Sounds.Add(SoundName.WallBump, new SoundBuffer("Resources/Sounds/77_arrowbounce.wav"));
            Sounds.Add(SoundName.BossSpotted, new SoundBuffer("Resources/Sounds/tindeck_1.wav"));
            Sounds.Add(SoundName.TrapSpotted, new SoundBuffer("Resources/Sounds/27_respawn2.wav"));
            Sounds.Add(SoundName.ItemUsed, new SoundBuffer("Resources/Sounds/66_drink.wav"));
            Sounds.Add(SoundName.Buff, new SoundBuffer("Resources/Sounds/79_arrowchain.wav"));
            Sounds.Add(SoundName.Debuff, new SoundBuffer("Resources/Sounds/74_bowrelease2.wav"));
            Sounds.Add(SoundName.Explosion, new SoundBuffer("Resources/Sounds/69_explode.wav"));
            Sounds.Add(SoundName.ItemPickedUp, new SoundBuffer("Resources/Sounds/14_item2.wav"));
            Sounds.Add(SoundName.SpellCast, new SoundBuffer("Resources/Sounds/16_falling.wav"));
            Sounds.Add(SoundName.MeleeAttack, new SoundBuffer("Resources/Sounds/23_ladder.wav"));
            Sounds.Add(SoundName.RangedAttack, new SoundBuffer("Resources/Sounds/33_rotate1.wav"));
            Sounds.Add(SoundName.MagicAttack, new SoundBuffer("Resources/Sounds/31_text.wav"));
            Sounds.Add(SoundName.EnemyDeath, new SoundBuffer("Resources/Sounds/69_explode.wav"));
            Sounds.Add(SoundName.PlayerDeath, new SoundBuffer("Resources/Sounds/64_lose2.wav"));
            Sounds.Add(SoundName.Countdown3, new SoundBuffer("Resources/Sounds/35_rotate3.wav"));
            Sounds.Add(SoundName.Countdown2, new SoundBuffer("Resources/Sounds/35_rotate3.wav"));
            Sounds.Add(SoundName.Countdown1, new SoundBuffer("Resources/Sounds/35_rotate3.wav"));

            await Localization.LoadJsonAsync(LocaleName.English, "Resources/Localizations/en/en.json");
            await Localization.LoadJsonAsync(LocaleName.Italian, "Resources/Localizations/it/it.json");
            Dialogues.LoadDialogues();

            await Sprites.LoadJsonAsync(TextureName.Creatures, "Resources/Spritesheets/creatures.json");
            await Sprites.LoadJsonAsync(TextureName.Items, "Resources/Spritesheets/items.json");
            await Sprites.LoadJsonAsync(TextureName.Features, "Resources/Spritesheets/features.json");
            await Sprites.LoadJsonAsync(TextureName.Tiles, "Resources/Spritesheets/tiles.json");
            await Sprites.LoadJsonAsync(TextureName.Spells, "Resources/Spritesheets/spells.json");
            await Sprites.LoadJsonAsync(TextureName.Animations, "Resources/Spritesheets/animations.json");
            await Sprites.LoadJsonAsync(TextureName.UI, "Resources/Spritesheets/ui.json");
            await Sprites.LoadJsonAsync(TextureName.Icons, "Resources/Spritesheets/icons.json");
            Sprites.BuildIndex(TextureName.FontMonospace, new(8, 8));
            Sprites.BuildIndex(TextureName.FontTerminal, new(8, 12));
            Sprites.BuildIndex(TextureName.FontLight, new(8, 12));

            await Colors.LoadJsonAsync("Resources/Palettes/default.json");

            foreach (var script in Enum.GetValues<ScriptName>())
                Scripts.TryLoad(script, out _);

            Store.SetValue(Data.View.TileSize, 16);
            Store.SetValue(Data.View.MinWindowSize, new(800, 800));
            Store.SetValue(Data.View.WindowSize, new(800, 800));
            Store.SetValue(Data.View.PopUpSize, new(400, 400));
            Store.SetValue(Data.View.DefaultForeground, Colors.Get(ColorName.UIPrimary));
            Store.SetValue(Data.View.DefaultBackground, Colors.Get(ColorName.UIBackground));
            Store.SetValue(Data.View.DefaultAccent, Colors.Get(ColorName.UIAccent));

            Store.SetValue(Data.Hotkeys.Cancel, VirtualKeys.Escape);
            Store.SetValue(Data.Hotkeys.Confirm, VirtualKeys.Return);
            Store.SetValue(Data.Hotkeys.Modifier, VirtualKeys.Control);
            Store.SetValue(Data.Hotkeys.Inventory, VirtualKeys.I);
            Store.SetValue(Data.Hotkeys.Interact, VirtualKeys.G);
            Store.SetValue(Data.Hotkeys.Look, VirtualKeys.X);
            Store.SetValue(Data.Hotkeys.Talk, VirtualKeys.K);
            Store.SetValue(Data.Hotkeys.AutoExplore, VirtualKeys.E);
            Store.SetValue(Data.Hotkeys.AutoFight, VirtualKeys.Tab);
            Store.SetValue(Data.Hotkeys.MoveNW, VirtualKeys.Numpad7);
            Store.SetValue(Data.Hotkeys.MoveN, VirtualKeys.Numpad8);
            Store.SetValue(Data.Hotkeys.MoveNE, VirtualKeys.Numpad9);
            Store.SetValue(Data.Hotkeys.MoveW, VirtualKeys.Numpad4);
            Store.SetValue(Data.Hotkeys.Wait, VirtualKeys.Numpad5);
            Store.SetValue(Data.Hotkeys.MoveE, VirtualKeys.Numpad6);
            Store.SetValue(Data.Hotkeys.MoveSW, VirtualKeys.Numpad1);
            Store.SetValue(Data.Hotkeys.MoveS, VirtualKeys.Numpad2);
            Store.SetValue(Data.Hotkeys.MoveSE, VirtualKeys.Numpad3);
            Store.SetValue(Data.Hotkeys.RotateTargetCw, VirtualKeys.Multiply);
            Store.SetValue(Data.Hotkeys.RotateTargetCCw, VirtualKeys.Divide);
            Store.SetValue(Data.Hotkeys.QuickSlot1, VirtualKeys.N1);
            Store.SetValue(Data.Hotkeys.QuickSlot2, VirtualKeys.N2);
            Store.SetValue(Data.Hotkeys.QuickSlot3, VirtualKeys.N3);
            Store.SetValue(Data.Hotkeys.QuickSlot4, VirtualKeys.N4);
            Store.SetValue(Data.Hotkeys.QuickSlot5, VirtualKeys.N5);
            Store.SetValue(Data.Hotkeys.QuickSlot6, VirtualKeys.N6);
            Store.SetValue(Data.Hotkeys.QuickSlot7, VirtualKeys.N7);
            Store.SetValue(Data.Hotkeys.QuickSlot8, VirtualKeys.N8);
            Store.SetValue(Data.Hotkeys.QuickSlot9, VirtualKeys.N9);
            Store.SetValue(Data.Hotkeys.ToggleZoom, VirtualKeys.Z);
            Store.SetValue(Data.Hotkeys.DeveloperConsole, VirtualKeys.F1);

            await Director.AddScenes(Scenes);
            Director.MapTransition(MenuScene.SceneState.Exit_NewGame, CharCreationScene.SceneState.Main);
            Director.MapTransition(CharCreationScene.SceneState.Exit_StartGame, GameplayScene.SceneState.Main);
            Director.MapTransition(CharCreationScene.SceneState.Exit_QuitToMenu, MenuScene.SceneState.Main);
            Director.TrySetState(MenuScene.SceneState.Main);
#if DEBUG
            // Start logging everything that passes through the global event bus
            var sieve = Bus.Filter<object>();
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    while (sieve.Messages.TryDequeue(out var msg))
                    {
                        var log = msg.ToString();
                        // Console.WriteLine(log);
                    }
                    await Task.Delay(100);
                }
            }, OffButton.Token);
            OffButton.Pressed += _ => sieve.Dispose();
#endif
        }
    }
}
