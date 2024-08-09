﻿

using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using Fiero.Core.Exceptions;
using SFML.Graphics;
using SFML.Window;
using System.Runtime;

namespace Fiero.Core
{
    //public class SpriteTermConverter<TTextures, TColors>(GameSprites<TTextures, TColors> sprites) : ITermConverter
    //    where TTextures : struct, Enum
    //    where TColors : struct, Enum
    //{
    //    protected readonly record struct SpriteTerm(TTextures Texture, TColors Color, string Sprite, int? RngSeed = null);

    //    public Type Type => typeof(Sprite);
    //    public TermMarshalling Marshalling => TermMarshalling.Named;

    //    public object FromTerm(ITerm t)
    //    {
    //        if (!t.Match(out SpriteTerm st))
    //            throw new NotSupportedException();
    //        return sprites.Get(st.Texture, st.Sprite, st.Color, rngSeed: st.RngSeed);
    //    }

    //    public ITerm ToTerm(object o, Maybe<Atom> overrideFunctor = default, Maybe<TermMarshalling> overrideMarshalling = default, TermMarshallingContext ctx = null)
    //    {
    //        if (o is not Sprite s)
    //            throw new NotSupportedException();
    //    }
    //}



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
        public readonly GameScripts Scripts;
        public readonly GameDirector Director;
        public readonly GameUI UI;
        public readonly GameWindow Window;
        public readonly GameLocalizations<TLocales> Localization;
        public readonly MetaSystem Meta;

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
            //if (!ValidateResources<string>(f => Scripts.TryGet(f, out _), out var missingScripts))
            //{
            //    throw new AggregateException(missingScripts.Select(x => new ResourceNotFoundException<TScripts>(x)));
            //}
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
            ValidateResources();
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
