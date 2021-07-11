//using Fiero.Core;
//using SFML.Graphics;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//namespace Fiero.Bio
//{

//    public class BioGame : Game<FontName, TextureName, LocaleName, SoundName, ColorName, ShaderName>
//    {
//        protected readonly IEnumerable<IGameScene> Scenes;

//        protected readonly GameDataStore Store;

//        public BioGame(
//            OffButton off,
//            GameLoop loop,
//            GameInput input,
//            GameTextures<TextureName> textures,
//            GameSprites<TextureName> sprites,
//            GameFonts<FontName> fonts,
//            GameSounds<SoundName> sounds,
//            GameColors<ColorName> colors,
//            GameShaders<ShaderName> shaders,
//            GameLocalizations<LocaleName> localization,
//            GameDirector director,
//            GameDataStore store,
//            IEnumerable<IGameScene> gameScenes)
//            : base(off, loop, input, textures, sprites, fonts, sounds, colors, shaders, localization, director)
//        {
//            Scenes = gameScenes;
//            Store = store;
//            loop.TimeStep = 1f / 1000;
//        }

//        protected override void InitializeWindow(RenderWindow win)
//        {
//            base.InitializeWindow(win);
//            win.Resized += (s, e) => {
//                var newSize = new Coord((int)e.Width, (int)e.Height).Clamp(min: 800);
//                if (newSize != new Coord((int)e.Width, (int)e.Height)) {
//                    win.Size = newSize;
//                }
//                else {
//                    Store.SetValue(Data.UI.WindowSize, newSize);
//                }
//            };
//            Store.SetValue(Data.UI.WindowSize, win.Size.ToCoord());
//        }

//        public override async Task InitializeAsync()
//        {
//            await base.InitializeAsync();

//            Fonts.Add(FontName.Default, new Font("Resources/Fonts/PressStart2P.ttf"));
//            await Localization.LoadJsonAsync(LocaleName.Default, "Resources/Localizations/en/en.json");
//            await Colors.LoadJsonAsync("Resources/Palettes/default.json");

//            Store.SetValue(Data.UI.TileSize, 8);
//            Store.SetValue(Data.UI.WindowSize, new(640, 480));

//            Director.AddScenes(Scenes);
//            Director.TrySetState(BioScene.SceneState.Main);
//        }
//    }
//}
