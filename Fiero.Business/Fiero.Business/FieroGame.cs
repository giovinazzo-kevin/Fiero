using Fiero.Business.Scenes;
using Fiero.Core;
using SFML.Audio;
using SFML.Graphics;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fiero.Business
{

    public class FieroGame : Game<FontName, TextureName, LocaleName, SoundName, ColorName, ShaderName>
    {
        protected readonly IEnumerable<IGameScene> Scenes;

        protected readonly GameDataStore Store;
        protected readonly GameGlossaries Glossary;
        protected readonly GameDialogues Dialogues;

        public FieroGame(
            OffButton off,
            GameLoop loop,
            GameInput input, 
            GameTextures<TextureName> textures,
            GameSprites<TextureName> sprites, 
            GameFonts<FontName> fonts,
            GameSounds<SoundName> sounds,
            GameColors<ColorName> colors,
            GameShaders<ShaderName> shaders,
            GameLocalizations<LocaleName> localization,
            GameDirector director,
            GameGlossaries glossary,
            GameDialogues dialogues,
            GameDataStore store,
            GameUI ui,
            GameWindow win,
            IEnumerable<IGameScene> gameScenes)
            : base(off, loop, input, textures, sprites, fonts, sounds, colors, shaders, localization, ui, win, director)
        {
            Dialogues = dialogues;
            Glossary = glossary;
            Scenes = gameScenes;
            Store = store;
            loop.TimeStep = 1f / 1000;
        }

        protected override void InitializeWindow(RenderWindow win)
        {
            base.InitializeWindow(win);
            win.Resized += (s, e) => {
                var newSize = new Coord((int)e.Width, (int)e.Height).Clamp(min: 800);
                if (newSize != new Coord((int)e.Width, (int)e.Height)) {
                    win.Size = newSize;
                }
                else {
                    Store.SetValue(Data.UI.WindowSize, newSize);
                }
            };
            Store.SetValue(Data.UI.WindowSize, win.Size.ToCoord());
        }

        protected override async Task InitializeAsync()
        {
            Fonts.Add(FontName.Bold, new(8, 8), new Texture("Resources/Fonts/CGA8x8thick.png"));
            Fonts.Add(FontName.Light, new(8, 8), new Texture("Resources/Fonts/CGA8x8thin.png"));

            Textures.Add(TextureName.Atlas, new Texture("Resources/Textures/8x8.png"));
            Textures.Add(TextureName.UI, new Texture("Resources/Textures/8x8_ui.png"));

            Shaders.Add(ShaderName.Test, new Shader(null, null, "Resources/Shaders/test.frag"));

            Sounds.Add(SoundName.UIBlip, new SoundBuffer("Resources/Sounds/UIBlip.ogg"));
            Sounds.Add(SoundName.UIOk, new SoundBuffer("Resources/Sounds/UIOk.ogg"));
            Sounds.Add(SoundName.PlayerDeath, new SoundBuffer("Resources/Sounds/Shutdown.ogg"));
            Sounds.Add(SoundName.WallBump, new SoundBuffer("Resources/Sounds/StoneMove.ogg"));
            Sounds.Add(SoundName.BossSpotted, new SoundBuffer("Resources/Sounds/Alarm Low.ogg"));

            await Localization.LoadJsonAsync(LocaleName.English, "Resources/Localizations/en/en.json");
            await Localization.LoadJsonAsync(LocaleName.Italian, "Resources/Localizations/it/it.json");

            await Sprites.LoadJsonAsync(TextureName.Atlas, "Resources/Spritesheets/atlas.json");
            await Sprites.LoadJsonAsync(TextureName.UI, "Resources/Spritesheets/ui.json");

            await Colors.LoadJsonAsync("Resources/Palettes/default.json");

            Glossary.LoadFactionGlossary(FactionName.Rats);
            Glossary.LoadFactionGlossary(FactionName.Snakes);
            Glossary.LoadFactionGlossary(FactionName.Cats);
            Glossary.LoadFactionGlossary(FactionName.Dogs);
            Glossary.LoadFactionGlossary(FactionName.Boars);

            Dialogues.LoadActorDialogues(NpcName.GreatKingRat);
            Dialogues.LoadFeatureDialogues(FeatureName.Shrine);

            Store.SetValue(Data.UI.TileSize, 8);
            Store.SetValue(Data.UI.WindowSize, new(800, 800));
            Store.SetValue(Data.UI.PopUpSize, new(400, 400));
            Store.SetValue(Data.UI.DefaultForeground, Colors.Get(ColorName.UIPrimary));
            Store.SetValue(Data.UI.DefaultBackground, Colors.Get(ColorName.UIBackground));
            Store.SetValue(Data.UI.DefaultAccent, Colors.Get(ColorName.UIAccent));

            Store.SetValue(Data.Hotkeys.Cancel, Keyboard.Key.Escape);
            Store.SetValue(Data.Hotkeys.Confirm, Keyboard.Key.Enter);
            Store.SetValue(Data.Hotkeys.Modifier, Keyboard.Key.LControl);
            Store.SetValue(Data.Hotkeys.Inventory, Keyboard.Key.I);
            Store.SetValue(Data.Hotkeys.Interact, Keyboard.Key.G);
            Store.SetValue(Data.Hotkeys.MoveNW, Keyboard.Key.Numpad7);
            Store.SetValue(Data.Hotkeys.MoveN, Keyboard.Key.Numpad8);
            Store.SetValue(Data.Hotkeys.MoveNE, Keyboard.Key.Numpad9);
            Store.SetValue(Data.Hotkeys.MoveW, Keyboard.Key.Numpad4);
            Store.SetValue(Data.Hotkeys.Wait, Keyboard.Key.Numpad5);
            Store.SetValue(Data.Hotkeys.MoveE, Keyboard.Key.Numpad6);
            Store.SetValue(Data.Hotkeys.MoveSW, Keyboard.Key.Numpad1);
            Store.SetValue(Data.Hotkeys.MoveS, Keyboard.Key.Numpad2);
            Store.SetValue(Data.Hotkeys.MoveSE, Keyboard.Key.Numpad3);

            await Director.AddScenes(Scenes);
            Director.MapTransition(MenuScene.SceneState.Exit_NewGame, GameplayScene.SceneState.Main);
            // Director.MapTransition(MenuScene.SceneState.Exit_Tracker, TrackerScene.SceneState.Main);
            Director.TrySetState(MenuScene.SceneState.Main);
        }
    }
}
