using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Fiero.Core
{
    public abstract class Game<TFonts, TTextures, TLocales, TSounds, TColors>
        where TFonts : struct, Enum
        where TTextures : struct, Enum
        where TLocales : struct, Enum
        where TSounds : struct, Enum
        where TColors : struct, Enum
    {
        public readonly OffButton OffButton;
        public readonly GameLoop Loop;
        public readonly GameInput Input;
        public readonly GameTextures<TTextures> Textures;
        public readonly GameSprites<TTextures> Sprites;
        public readonly GameColors<TColors> Colors;
        public readonly GameFonts<TFonts> Fonts;
        public readonly GameSounds<TSounds> Sounds;
        public readonly GameDirector Director;
        public readonly GameLocalizations<TLocales> Localization;

        public Game(OffButton off, GameLoop loop, GameInput input, GameTextures<TTextures> resources, GameSprites<TTextures> sprites, GameFonts<TFonts> fonts, GameSounds<TSounds> sounds, GameColors<TColors> colors, GameDirector director, GameLocalizations<TLocales> localization)
        {
            OffButton = off;
            Loop = loop;
            Input = input;
            Colors = colors;
            Textures = resources;
            Sprites = sprites;
            Sounds = sounds;
            Fonts = fonts;
            Director = director;
            Localization = localization;
        }

        public virtual Task InitializeAsync()
        {
            Sounds.Initialize();
            return Task.CompletedTask;
        }

        protected virtual void InitializeWindow(RenderWindow win)
        {
            win.SetKeyRepeatEnabled(true);
            win.SetActive(true);
            win.Resized += (e, eh) => {
                win.GetView()?.Dispose();
                win.SetView(new(new FloatRect(0, 0, eh.Width, eh.Height)));
            };
        }

        public void Run(CancellationToken token = default)
        {
            using var win = new RenderWindow(new VideoMode(800, 800), String.Empty);
            InitializeWindow(win);
            Loop.Tick += (t, dt) => {
                win.DispatchEvents();
            };
            Loop.Update += (t, dt) => {
                Update(win, t, dt);
            };
            // Always called once per frame before the window is drawn
            Loop.Render += (t, dt) => {
                Draw(win, t, dt);
            };
            Loop.Run(token);
        }

        public virtual void Update(RenderWindow win, float t, float dt)
        {
            if(win.HasFocus()) {
                Input.Update(Mouse.GetPosition(win));
            }
            Director.Update(win, t, dt);
        }

        public virtual void Draw(RenderWindow win, float t, float dt)
        {
            Director.Draw(win, t, dt);
            win.Display();
        }
    }
}
