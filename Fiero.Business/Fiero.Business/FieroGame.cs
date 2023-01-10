using Fiero.Business.Scenes;
using Fiero.Core;
using SFML.Audio;
using SFML.Graphics;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unconcern.Common;

namespace Fiero.Business
{
    public class FieroGame : Game<FontName, TextureName, LocaleName, SoundName, ColorName, ShaderName>
    {
        protected readonly IEnumerable<IGameScene> Scenes;

        protected readonly EventBus Bus;
        protected readonly GameDataStore Store;
        protected readonly GameGlossaries Glossary;
        protected readonly GameDialogues Dialogues;

        public FieroGame(
            EventBus bus,
            OffButton off,
            GameLoop loop,
            GameInput input,
            GameDirector director,
            GameGlossaries glossary,
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
            Glossary = glossary;
            Scenes = gameScenes;
            Store = store;
            loop.TimeStep = 1f / 1000;
        }

        protected override void InitializeWindow(RenderWindow win)
        {
            base.InitializeWindow(win);
            win.Resized += (s, e) =>
            {
                var newSize = new Coord((int)e.Width, (int)e.Height).Clamp(min: 800);
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
            Textures.Add(TextureName.FontBold, new Texture("Resources/Fonts/CGA8x8thick.png"));
            Textures.Add(TextureName.FontLight, new Texture("Resources/Fonts/CGA8x8thin.png"));

            Fonts.Add(FontName.Bold, new(8, 8), Textures.Get(TextureName.FontBold));
            Fonts.Add(FontName.Light, new(8, 8), Textures.Get(TextureName.FontLight));

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
            await Sprites.LoadJsonAsync(TextureName.FontBold, "Resources/Spritesheets/index.json");
            await Sprites.LoadJsonAsync(TextureName.FontLight, "Resources/Spritesheets/index.json");

            await Colors.LoadJsonAsync("Resources/Palettes/default.json");

            Glossary.LoadFactionGlossary(FactionName.Rats);
            Glossary.LoadFactionGlossary(FactionName.Snakes);
            Glossary.LoadFactionGlossary(FactionName.Cats);
            Glossary.LoadFactionGlossary(FactionName.Dogs);
            Glossary.LoadFactionGlossary(FactionName.Boars);

            Dialogues.LoadActorDialogues(NpcName.GreatKingRat);
            Dialogues.LoadFeatureDialogues(FeatureName.Shrine);

            Store.SetValue(Data.UI.TileSize, 8);
            Store.SetValue(Data.UI.WindowSize, new(640, 480));
            Store.SetValue(Data.UI.PopUpSize, new(200, 200));
            Store.SetValue(Data.UI.DefaultForeground, Colors.Get(ColorName.UIPrimary));
            Store.SetValue(Data.UI.DefaultBackground, Colors.Get(ColorName.UIBackground));
            Store.SetValue(Data.UI.DefaultAccent, Colors.Get(ColorName.UIAccent));

            Store.SetValue(Data.Hotkeys.Cancel, Keyboard.Key.Escape);
            Store.SetValue(Data.Hotkeys.Confirm, Keyboard.Key.Enter);
            Store.SetValue(Data.Hotkeys.Modifier, Keyboard.Key.LControl);
            Store.SetValue(Data.Hotkeys.Inventory, Keyboard.Key.I);
            Store.SetValue(Data.Hotkeys.Interact, Keyboard.Key.G);
            Store.SetValue(Data.Hotkeys.Look, Keyboard.Key.X);
            Store.SetValue(Data.Hotkeys.MoveNW, Keyboard.Key.Numpad7);
            Store.SetValue(Data.Hotkeys.MoveN, Keyboard.Key.Numpad8);
            Store.SetValue(Data.Hotkeys.MoveNE, Keyboard.Key.Numpad9);
            Store.SetValue(Data.Hotkeys.MoveW, Keyboard.Key.Numpad4);
            Store.SetValue(Data.Hotkeys.Wait, Keyboard.Key.Numpad5);
            Store.SetValue(Data.Hotkeys.MoveE, Keyboard.Key.Numpad6);
            Store.SetValue(Data.Hotkeys.MoveSW, Keyboard.Key.Numpad1);
            Store.SetValue(Data.Hotkeys.MoveS, Keyboard.Key.Numpad2);
            Store.SetValue(Data.Hotkeys.MoveSE, Keyboard.Key.Numpad3);
            Store.SetValue(Data.Hotkeys.RotateTargetCw, Keyboard.Key.Multiply);
            Store.SetValue(Data.Hotkeys.RotateTargetCCw, Keyboard.Key.Divide);
            Store.SetValue(Data.Hotkeys.QuickSlot1, Keyboard.Key.Num1);
            Store.SetValue(Data.Hotkeys.QuickSlot2, Keyboard.Key.Num2);
            Store.SetValue(Data.Hotkeys.QuickSlot3, Keyboard.Key.Num3);
            Store.SetValue(Data.Hotkeys.QuickSlot4, Keyboard.Key.Num4);
            Store.SetValue(Data.Hotkeys.QuickSlot5, Keyboard.Key.Num5);
            Store.SetValue(Data.Hotkeys.QuickSlot6, Keyboard.Key.Num6);
            Store.SetValue(Data.Hotkeys.QuickSlot7, Keyboard.Key.Num7);
            Store.SetValue(Data.Hotkeys.QuickSlot8, Keyboard.Key.Num8);
            Store.SetValue(Data.Hotkeys.QuickSlot9, Keyboard.Key.Num9);
            Store.SetValue(Data.Hotkeys.ToggleZoom, Keyboard.Key.Z);

            await Director.AddScenes(Scenes);
            Director.MapTransition(MenuScene.SceneState.Exit_NewGame, GameplayScene.SceneState.Main);
            Director.TrySetState(MenuScene.SceneState.Main);

#if DEBUG
            // Start logging everything that passes through the global event bus
            var sieve = Bus.Filter<object>();
            _ = Task.Run(async () =>
            {
                var logPath = "log.txt";
                if (File.Exists(logPath))
                {
                    File.Delete(logPath);
                }
                while (true)
                {
                    while (sieve.Messages.TryDequeue(out var msg))
                    {
                        var log = msg.ToString();
                        if (log.Contains("Player"))
                        {
                            Console.WriteLine(log);
                        }
                    }
                    await Task.Delay(10);
                }
            }, OffButton.Token);
            OffButton.Pressed += _ => sieve.Dispose();
#endif
        }
    }
}
