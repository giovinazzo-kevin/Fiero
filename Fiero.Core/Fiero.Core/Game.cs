using Fiero.Core.Exceptions;
using Fiero.Core.Structures;
using SFML.Graphics;
using SFML.Window;
using System.Diagnostics;

namespace Fiero.Core
{
    public abstract class Game<TFonts, TTextures, TLocales, TSounds, TColors, TShaders>
        : IGame
        where TFonts : struct, Enum
        where TTextures : struct, Enum
        where TLocales : struct, Enum
        where TSounds : struct, Enum
        where TColors : struct, Enum
        where TShaders : struct, Enum
    {
        public readonly OffButton OffButton;
        public readonly GameLoop Loop;
        public readonly GameInput Input;
        public readonly GameTextures<TTextures> Textures;
        public readonly GameSprites<TTextures, TColors> Sprites;
        public readonly GameColors<TColors> Colors;
        public readonly GameFonts<TFonts> Fonts;
        public readonly GameSounds<TSounds> Sounds;
        public readonly GameShaders<TShaders> Shaders;
        public readonly GameDirector Director;
        public readonly GameUI UI;
        public readonly GameWindow Window;
        public readonly GameLocalizations<TLocales> Localization;

        public float MeasuredFramesPerSecond { get; private set; }
        private readonly Stopwatch _fpsStopwatch = new();
        private TimeSpan _lastTimestamp;

        public Game(
            OffButton off,
            GameLoop loop,
            GameInput input,
            GameTextures<TTextures> resources,
            GameSprites<TTextures, TColors> sprites,
            GameFonts<TFonts> fonts,
            GameSounds<TSounds> sounds,
            GameColors<TColors> colors,
            GameShaders<TShaders> shaders,
            GameLocalizations<TLocales> localization,
            GameUI ui,
            GameWindow window,
            GameDirector director)
        {
            OffButton = off;
            Loop = loop;
            Input = input;
            Colors = colors;
            Shaders = shaders;
            Textures = resources;
            Sprites = sprites;
            Sounds = sounds;
            Fonts = fonts;
            UI = ui;
            Window = window;
            Director = director;
            Localization = localization;
        }
        protected abstract Task InitializeAsync();
        protected bool ValidateResources<TEnum>(Func<TEnum, bool> validate, out IEnumerable<TEnum> failures)
            where TEnum : struct, Enum
        {
            failures = Enum.GetValues<TEnum>().Where(x => !validate(x));
            return !failures.Any();
        }

        protected virtual void ValidateResources()
        {
            if (!ValidateResources<TFonts>(f => Fonts.Get(f) != null, out var missingFonts))
            {
                throw new AggregateException(missingFonts.Select(x => new ResourceNotFoundException<TFonts>(x)));
            }
            if (!ValidateResources<TTextures>(f => Textures.Get(f) != null, out var missingTextures))
            {
                throw new AggregateException(missingTextures.Select(x => new ResourceNotFoundException<TTextures>(x)));
            }
            if (!ValidateResources<TColors>(f => Colors.TryGet(f, out _), out var missingColors))
            {
                throw new AggregateException(missingColors.Select(x => new ResourceNotFoundException<TColors>(x)));
            }
            if (!ValidateResources<TSounds>(f => Sounds.Get(f) != null, out var missingSounds))
            {
                throw new AggregateException(missingSounds.Select(x => new ResourceNotFoundException<TSounds>(x)));
            }
            if (!ValidateResources<TShaders>(f => Shaders.Get(f) != null, out var missingShaders))
            {
                throw new AggregateException(missingShaders.Select(x => new ResourceNotFoundException<TShaders>(x)));
            }
            if (!ValidateResources<TLocales>(f => Localization.HasLocale(f), out var missingLocales))
            {
                throw new AggregateException(missingLocales.Select(x => new ResourceNotFoundException<TLocales>(x)));
            }
        }

        protected virtual void InitializeWindow(RenderWindow win)
        {
            win.SetFramerateLimit(144);
            win.SetKeyRepeatEnabled(true);
            win.SetActive(true);
            win.Resized += (e, eh) =>
            {
                win.GetView()?.Dispose();
                win.SetView(new(new FloatRect(0, 0, eh.Width, eh.Height)));
            };
        }

        public async Task RunAsync(CancellationToken token = default)
        {
            await InitializeAsync();
            ValidateResources();
            using (Window.RenderWindow = new RenderWindow(new VideoMode(800, 800), String.Empty))
            {
                InitializeWindow(Window.RenderWindow);
                Loop.Tick += (t, dt) =>
                {
                    Window.DispatchEvents();
                };
                Loop.Update += (t, dt) =>
                {
                    Update();
                    Input.Update();
                };
                // Always called once per frame before the window is drawn
                Loop.Render += (t, dt) =>
                {
                    var states = RenderStates.Default;
                    Draw(Window.RenderWindow, states);
                    Window.Display();
                };
                _fpsStopwatch.Start();
                Loop.Run(ct: token);
            }
        }

        public virtual void Update()
        {
            if (Window.HasFocus())
            {
                // Update all non-modal windows
                foreach (var wnd in UI.GetOpenWindows().Reverse())
                {
                    wnd.Update();
                }
                // Then update the topmost modal
                if (UI.GetOpenModals().LastOrDefault() is { } modal)
                {
                    modal.Update();
                }
                // If no modal is open, just update the current scene
                else
                {
                    Director.Update();
                }
            }
        }

        public virtual void Draw(RenderTarget target, RenderStates states)
        {
            var currentTimestamp = _fpsStopwatch.Elapsed;
            var frameTime = currentTimestamp - _lastTimestamp;
            _lastTimestamp = currentTimestamp;

            MeasuredFramesPerSecond = 0.02f * 1f / (float)frameTime.TotalSeconds + MeasuredFramesPerSecond * 0.98f;

            Director.Draw();
            // Windows are drawn before modals
            foreach (var win in UI.GetOpenWindows().Union(UI.GetOpenModals()))
            {
                win.Draw(target, states);
            }
            using var text = new Text($"FPS: {MeasuredFramesPerSecond:000.0}", new Font(@"C:\Windows\Fonts\Arial.ttf"));
            target.Draw(text);
        }
    }
}
