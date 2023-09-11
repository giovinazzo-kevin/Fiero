using Fiero.Business.Scenes;
using Fiero.Core;
using Fiero.Core.Structures;
using SFML.Audio;
using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unconcern.Common;

namespace Fiero.Business
{
    public class FieroGame : Game<FontName, TextureName, LocaleName, SoundName, ColorName, ShaderName>
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
            GameColors<ColorName> colors,
            GameShaders<ShaderName> shaders,
            GameLocalizations<LocaleName> localization,
            IEnumerable<IGameScene> gameScenes)
            : base(off, loop, input, textures, sprites, fonts, sounds, colors, shaders, localization, ui, win, director)
        {
            Bus = bus;
            Dialogues = dialogues;
            Scenes = gameScenes;
            Store = store;
            loop.TimeStep = TimeSpan.FromSeconds(1 / 200f);

            Data.UI.WindowSize.ValueChanged += WindowSize_ValueChanged;
            void WindowSize_ValueChanged(GameDatumChangedEventArgs<Coord> obj)
            {
                UI.Store.SetValue(Data.UI.ViewportSize, obj.NewValue - new Coord(248, 128));
            }
        }

        protected override void InitializeWindow(RenderWindow win)
        {
            base.InitializeWindow(win);
            win.Resized += (s, e) =>
            {
                var minSize = Store.Get(Data.UI.MinWindowSize);
                var newSize = new Coord((int)e.Width, (int)e.Height).Clamp(minX: minSize.X, minY: minSize.Y);
                if (newSize != new Coord((int)e.Width, (int)e.Height))
                {
                    win.Size = newSize;
                }
                else
                {
                    Store.SetValue(Data.UI.WindowSize, newSize);
                }
            };
            Store.SetValue(Data.UI.WindowSize, win.Size.ToCoord());
        }

        protected override async Task InitializeAsync()
        {
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

            Sounds.Add(SoundName.UIBlip, new SoundBuffer("Resources/Sounds/00_start1.wav"));
            Sounds.Add(SoundName.UIOk, new SoundBuffer("Resources/Sounds/00_start1.wav"));
            Sounds.Add(SoundName.WallBump, new SoundBuffer("Resources/Sounds/77_arrowbounce.wav"));
            Sounds.Add(SoundName.BossSpotted, new SoundBuffer("Resources/Sounds/62_miss.wav"));
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

            await Localization.LoadJsonAsync(LocaleName.English, "Resources/Localizations/en/en.json");
            await Localization.LoadJsonAsync(LocaleName.Italian, "Resources/Localizations/it/it.json");

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

            Dialogues.LoadActorDialogues(NpcName.GreatKingRat);
            Dialogues.LoadFeatureDialogues(FeatureName.Shrine);

            Store.SetValue(Data.UI.TileSize, 16);
            Store.SetValue(Data.UI.MinWindowSize, new(800, 800));
            Store.SetValue(Data.UI.WindowSize, new(800, 800));
            Store.SetValue(Data.UI.PopUpSize, new(400, 400));
            Store.SetValue(Data.UI.DefaultForeground, Colors.Get(ColorName.UIPrimary));
            Store.SetValue(Data.UI.DefaultBackground, Colors.Get(ColorName.UIBackground));
            Store.SetValue(Data.UI.DefaultAccent, Colors.Get(ColorName.UIAccent));

            Store.SetValue(Data.Hotkeys.Cancel, VirtualKeys.Escape);
            Store.SetValue(Data.Hotkeys.Confirm, VirtualKeys.Return);
            Store.SetValue(Data.Hotkeys.Modifier, VirtualKeys.Control);
            Store.SetValue(Data.Hotkeys.Inventory, VirtualKeys.I);
            Store.SetValue(Data.Hotkeys.Interact, VirtualKeys.G);
            Store.SetValue(Data.Hotkeys.Look, VirtualKeys.X);
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
            Director.MapTransition(MenuScene.SceneState.Exit_NewGame, GameplayScene.SceneState.Main);
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
