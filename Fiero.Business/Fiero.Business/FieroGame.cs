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

    public class FieroGame : Game<FontName, TextureName, LocaleName, SoundName, ColorName>
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
            GameDirector director,
            GameGlossaries glossary,
            GameDialogues dialogues,
            GameDataStore store,
            GameLocalizations<LocaleName> localization,
            IEnumerable<IGameScene> gameScenes)
            : base(off, loop, input, textures, sprites, fonts, sounds, colors, director, localization)
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
                var newSize = new Coord((int)e.Width, (int)e.Height);
                Store.SetValue(Data.UI.WindowSize, newSize);
            };
            Store.SetValue(Data.UI.WindowSize, win.Size.ToCoord());
        }

        protected virtual void InitializeStore()
        {
            Store.SetValue(Data.UI.TileSize, 8);
            Store.SetValue(Data.UI.WindowSize, new(640, 480));
            Store.SetValue(Data.UI.DefaultActiveForeground, Colors.Get(ColorName.UIPrimary));
            Store.SetValue(Data.UI.DefaultInactiveForeground, Colors.Get(ColorName.UISecondary));
            Store.SetValue(Data.UI.DefaultActiveBackground, Colors.Get(ColorName.UIBackground));
            Store.SetValue(Data.UI.DefaultInactiveBackground, Colors.Get(ColorName.UIBackground));
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            Fonts.Add(FontName.UI, new Font("Resources/Fonts/PressStart2P.ttf"));
            Textures.Add(TextureName.Atlas, new Texture("Resources/Textures/8x8.png"));
            Textures.Add(TextureName.UI, new Texture("Resources/Textures/8x8_ui.png"));

            Sounds.Add(SoundName.UIBlip, new SoundBuffer("Resources/Sounds/UIBlip.ogg"));
            Sounds.Add(SoundName.UIOk, new SoundBuffer("Resources/Sounds/UIOk.ogg"));
            Sounds.Add(SoundName.PlayerDeath, new SoundBuffer("Resources/Sounds/Oof.ogg"));
            Sounds.Add(SoundName.PlayerMove, new SoundBuffer("Resources/Sounds/StoneMove.ogg"));

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

            Dialogues.LoadActorDialogues(ActorName.GreatKingRat);
            Dialogues.LoadFeatureDialogues(FeatureName.Shrine);

            InitializeStore();

            Director.AddScenes(Scenes);
            Director.MapTransition(MenuScene.SceneState.Exit_NewGame, GameplayScene.SceneState.Main);
            Director.TrySetState(MenuScene.SceneState.Main);
        }
    }
}
