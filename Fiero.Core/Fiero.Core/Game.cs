

using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using Fiero.Core.Exceptions;
using SFML.Graphics;
using SFML.Window;
using System.Runtime;

namespace Fiero.Core
{


    public abstract class Game
        : IGame
    {
        public readonly OffButton OffButton;
        public readonly GameLoop Loop;
        public readonly GameInput Input;
        public readonly GameTextures Textures;
        public readonly GameSprites Sprites;
        public readonly GameColors Colors;
        public readonly GameFonts Fonts;
        public readonly GameSounds Sounds;
        public readonly GameShaders Shaders;
        public readonly GameScripts Scripts;
        public readonly GameDirector Director;
        public readonly GameUI UI;
        public readonly GameWindow Window;
        public readonly GameLocalizations Localization;
        public readonly MetaSystem Meta;

        public Game(
            OffButton off,
            GameLoop loop,
            GameInput input,
            GameTextures resources,
            GameSprites sprites,
            GameFonts fonts,
            GameSounds sounds,
            GameColors colors,
            GameShaders shaders,
            GameLocalizations localization,
            GameScripts scripts,
            GameUI ui,
            GameWindow window,
            GameDirector director,
            GameEntities entities,
            MetaSystem meta)
        {
            OffButton = off;
            Loop = loop;
            Input = input;
            Colors = colors;
            Shaders = shaders;
            Textures = resources;
            Scripts = scripts;
            Sprites = sprites;
            Sounds = sounds;
            Fonts = fonts;
            UI = ui;
            Window = window;
            Director = director;
            Localization = localization;
            Meta = meta;
            // Hydrate entities when parsed from terms
            // TODO: Figure out overlap with EntityAsTerm
            TermMarshall.RegisterTransform<EcsEntity>(e =>
            {
                var proxyType = e.GetType();
                if (entities.TryGetProxy(proxyType, e.Id, out var proxy))
                    return proxy;
                return e;
            });
        }
        protected virtual async Task InitializeAsync()
        {
            Meta.Initialize();
            await Task.CompletedTask;
        }
        protected bool ValidateResources<TEnum>(Func<TEnum, bool> validate, out IEnumerable<TEnum> failures)
            where TEnum : struct, Enum
        {
            failures = Enum.GetValues<TEnum>().Where(x => !validate(x));
            return !failures.Any();
        }

        protected virtual void InitializeWindow(RenderWindow win)
        {
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
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            await InitializeAsync();
            // ValidateResources();
            using (Window.RenderWindow = new RenderWindow(new VideoMode(800, 800), String.Empty))
            {
                InitializeWindow(Window.RenderWindow);
                Loop.Update += (t, dt) =>
                {
                    Update(t, dt);
                    Input.Update();
                };
                // Always called once per frame before the window is drawn
                Loop.Render += (t, dt) =>
                {
                    Window.DispatchEvents();
                    var states = RenderStates.Default;
                    Draw(Window.RenderWindow, states);
                    Window.Display();
                };
                Loop.Run(ct: token);
            }
        }

        public virtual void Update(TimeSpan t, TimeSpan dt)
        {
            if (Window.HasFocus())
            {
                // Update all non-modal windows
                foreach (var wnd in UI.GetOpenWindows().Reverse())
                {
                    wnd.Update(t, dt);
                }
                // Then update the topmost modal
                if (UI.GetOpenModals().LastOrDefault() is { } modal)
                {
                    modal.Update(t, dt);
                }
                // If no modal is open, just update the current scene
                else
                {
                    Director.Update(t, dt);
                }
            }
        }

        Font arial = new Font(@"C:\Windows\Fonts\Arial.ttf");
        public virtual void Draw(RenderTarget target, RenderStates states)
        {
            Director.DrawBackground(target, states);
            // Windows are drawn before modals
            foreach (var win in UI.GetOpenWindows().Union(UI.GetOpenModals()))
            {
                win.Draw(target, states);
            }
            Director.DrawForeground(target, states);
            using var text = new Text($"FPS: {Loop.FPS:000.0}", arial);
            target.Draw(text);
        }
    }
}
