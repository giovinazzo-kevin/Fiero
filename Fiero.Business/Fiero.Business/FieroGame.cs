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
            : base(loop, input, textures, sprites, fonts, sounds, colors, director, localization)
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
            Data.UI.WindowSize.ValueChanged += e => {
                win.Size = new((uint)e.NewValue.X, (uint)e.NewValue.Y);
            };

            var cfgCheck = Store.TrySetValue(Data.UI.TileSize, default, 8)
                        && Store.TrySetValue(Data.UI.WindowSize, default, new(640, 480))
                        && Store.TrySetValue(Data.UI.DefaultActiveColor, default, new(255, 255, 255, 255))
                        && Store.TrySetValue(Data.UI.DefaultInactiveColor, default, new(128, 128, 128, 255));
            if (!cfgCheck) {
                throw new InvalidOperationException();
            }
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            Fonts.Add(FontName.UI, new Font("Resources/Fonts/PressStart2P.ttf"));
            Textures.Add(TextureName.Atlas, new Texture("Resources/Textures/8x8.png"));
            Textures.Add(TextureName.UI, new Texture("Resources/Textures/8x8_ui.png"));

            Sounds.Add(SoundName.UIBlip, new SoundBuffer("Resources/Sounds/UIBlip.ogg"));
            Sounds.Add(SoundName.UIOk, new SoundBuffer("Resources/Sounds/UIOk.ogg"));
            Sounds.Add(SoundName.Oof, new SoundBuffer("Resources/Sounds/Oof.ogg"));

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

            Director.AddScenes(Scenes);
            Director.MapTransition(MenuScene.SceneState.Exit_NewGame, GameplayScene.SceneState.Main);
            Director.TrySetState(MenuScene.SceneState.Main);
        }

        public override void Update(RenderWindow win, float t, float dt)
        {
            base.Update(win, t, dt);
        }

        public override void Draw(RenderWindow win, float t, float dt)
        {
            base.Draw(win, t, dt);
        }
    }
}
