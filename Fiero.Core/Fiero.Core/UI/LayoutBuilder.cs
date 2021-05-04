using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Fiero.Core
{
    public class LayoutBuilder<TFonts, TTextures, TSounds>
        where TFonts : struct, Enum
        where TTextures : struct, Enum
        where TSounds : struct, Enum
    {
        private static readonly string[] _frameLabels = new[] {
            "tl", "tm", "tr", "l", "m", "r", "bl", "bm", "br"
        };

        public readonly GameSprites<TTextures> Sprites;
        public readonly GameFonts<TFonts> Fonts;
        public readonly GameSounds<TSounds> Sounds;
        public readonly GameDataStore Store;
        public readonly GameInput Input;

        public readonly List<Func<UIControl>> Controls;

        public TFonts CurrentFont { get; private set; }
        public TTextures CurrentTexture { get; private set; }
        public int CurrentTileSize { get; private set; }

        internal LayoutBuilder(GameInput input, GameFonts<TFonts> fonts, GameSprites<TTextures> sprites, GameSounds<TSounds> sounds, GameDataStore store)
        {
            Controls = new List<Func<UIControl>>();
            Sprites = sprites;
            Sounds = sounds;
            Fonts = fonts;
            Input = input;
            Store = store;
        }

        public LayoutBuilder<TFonts, TTextures, TSounds> WithFont(TFonts font)
        {
            CurrentFont = font;
            return this;
        }

        public LayoutBuilder<TFonts, TTextures, TSounds> WithTexture(TTextures tex)
        {
            CurrentTexture = tex;
            return this;
        }

        public LayoutBuilder<TFonts, TTextures, TSounds> WithTileSize(int tileSize)
        {
            CurrentTileSize = tileSize;
            return this;
        }

        public LayoutBuilder<TFonts, TTextures, TSounds> Textbox(Coord position, int length, string defaultText = "", Color? activeColor = null, Color? inactiveColor = null, string spriteName = "textbox", Action<Textbox> initialize = null)
        {
            var (tileSize, texture, font) = (CurrentTileSize, CurrentTexture, CurrentFont);
            Controls.Add(() => {
                var frame = CreateFrame(texture, spriteName, new(tileSize * length, tileSize));
                frame.ActiveColor = activeColor ?? Color.White;
                frame.InactiveColor = inactiveColor ?? new Color(192, 192, 192, 255);

                var textbox = new Textbox(Input, frame, length, text => new Text(text, Fonts.Get(font), (uint)tileSize)) {
                    Text = defaultText,
                    Position = new(position.X * tileSize, position.Y * tileSize),
                    Size = new(length * tileSize, tileSize),
                    ActiveColor = frame.ActiveColor,
                    InactiveColor = frame.InactiveColor
                };
                initialize?.Invoke(textbox);
                return textbox;
            });
            return this;
        }

        public LayoutBuilder<TFonts, TTextures, TSounds> Button(Coord position, int length, string defaultText = "", Color? activeColor = null, Color? inactiveColor = null, string spriteName = "button", Action<Button> initialize = null)
        {
            var (tileSize, texture, font) = (CurrentTileSize, CurrentTexture, CurrentFont);
            Controls.Add(() => {
                var frame = CreateFrame(texture, spriteName, new(tileSize * length, tileSize));
                frame.ActiveColor = activeColor ?? Color.White;
                frame.InactiveColor = inactiveColor ?? new Color(192, 192, 192, 255);

                var button = new Button(Input, frame, length, text => new Text(text, Fonts.Get(font), (uint)tileSize)) {
                    Text = defaultText,
                    Position = new(position.X * tileSize, position.Y * tileSize),
                    Size = new(length * tileSize, tileSize),
                    ActiveColor = frame.ActiveColor,
                    InactiveColor = frame.InactiveColor
                };
                initialize?.Invoke(button);
                return button;
            });
            return this;
        }
        public LayoutBuilder<TFonts, TTextures, TSounds> Label(Coord position, int length, string defaultText = "", Color? activeColor = null, Color? inactiveColor = null, Action<Label> initialize = null)
        {
            var (tileSize, texture, font) = (CurrentTileSize, CurrentTexture, CurrentFont);
            Controls.Add(() => {
                var label = new Label(Input, length, text => new Text(text, Fonts.Get(font), (uint)tileSize)) {
                    Text = defaultText,
                    Position = new(position.X * tileSize, position.Y * tileSize),
                    Size = new(length * tileSize, tileSize),
                    ActiveColor = activeColor ?? Color.White,
                    InactiveColor = inactiveColor ?? new Color(192, 192, 192, 255)
                };
                initialize?.Invoke(label);
                return label;
            });
            return this;
        }
        public LayoutBuilder<TFonts, TTextures, TSounds> Paragraph(Coord position, int length, int lines, string defaultText = "", string spriteName = "conversation", Color? activeColor = null, Color? inactiveColor = null, Action<Paragraph> initialize = null)
        {
            var (tileSize, texture, font) = (CurrentTileSize, CurrentTexture, CurrentFont);
            Controls.Add(() => {
                var frame = CreateFrame(texture, spriteName, new(tileSize * length, (int)(tileSize * 1.5 * lines)));
                frame.ActiveColor = activeColor ?? Color.White;
                frame.InactiveColor = inactiveColor ?? new Color(192, 192, 192, 255);

                var paragraph = new Paragraph(Input, frame, length, lines, text => new Text(text, Fonts.Get(font), (uint)tileSize)) {
                    Text = defaultText,
                    Position = new(position.X * tileSize, position.Y * tileSize),
                    Size = new(length * tileSize, lines * tileSize),
                    ActiveColor = activeColor ?? Color.White,
                    InactiveColor = inactiveColor ?? new Color(192, 192, 192, 255)
                };
                initialize?.Invoke(paragraph);
                return paragraph;
            });
            return this;
        }
        public LayoutBuilder<TFonts, TTextures, TSounds> ProgressBar(Coord position, int length, float defaultProgress = 0, string spriteName = "bar", Color? activeColor = null, Color? inactiveColor = null, Action<ProgressBar> initialize = null)
        {
            var (tileSize, texture, font) = (CurrentTileSize, CurrentTexture, CurrentFont);
            Controls.Add(() => {
                var sprites = new[] { "empty", "half", "full" }
                    .SelectMany(underscore => new[] { "l", "m", "r" }
                        .Select(suffix => Sprites.TryGet(texture, $"{spriteName}_{underscore}-{suffix}", out var s) ? s : null))
                    .ToArray();

                var progressBar = new ProgressBar(Input, tileSize, length, sprites[0], sprites[1], sprites[2], sprites[3], sprites[4], sprites[5], sprites[6], sprites[7], sprites[8]) {
                    Progress = defaultProgress,
                    Position = new(position.X * tileSize, position.Y * tileSize),
                    Size = new(length * tileSize, tileSize),
                    ActiveColor = activeColor ?? Color.White,
                    InactiveColor = inactiveColor ?? new Color(192, 192, 192, 255)
                };
                initialize?.Invoke(progressBar);
                return progressBar;
            });
            return this;
        }

        public LayoutBuilder<TFonts, TTextures, TSounds> Combobox<TValue>(Coord position, int length, Color? activeColor = null, Color? inactiveColor = null, string spriteName = "combo", string itemSpriteName = "combo_item", Action<Combobox<TValue>> initialize = null, Action<ComboItem> initializeItem = null)
        {
            var (tileSize, texture, font) = (CurrentTileSize, CurrentTexture, CurrentFont);
            Controls.Add(() => {
                var frame = CreateFrame(texture, spriteName, new(tileSize * length, tileSize));
                frame.ActiveColor = activeColor ?? Color.White;
                frame.InactiveColor = inactiveColor ?? new Color(192, 192, 192, 255);

                Func<string, Text> getText = text => new Text(text, Fonts.Get(font), (uint)tileSize);
                Func<ComboItem> getItem = () => {
                    var innerFrame = CreateFrame(texture, itemSpriteName, new(tileSize * length, tileSize));
                    innerFrame.ActiveColor = frame.ActiveColor;
                    innerFrame.InactiveColor = frame.InactiveColor;
                    var item = new ComboItem(Input, innerFrame, length, getText);
                    initializeItem?.Invoke(item);
                    return item;
                };

                var combobox = new Combobox<TValue>(Input, frame, length, getText, getItem) {
                    Position = new(position.X * tileSize, position.Y * tileSize),
                    Size = new(length * tileSize, tileSize),
                    ActiveColor = frame.ActiveColor,
                    InactiveColor = frame.InactiveColor
                };
                initialize?.Invoke(combobox);
                return combobox;
            });
            return this;
        }

        public Layout Build() => new(Input, Controls.Select(c => c()).ToArray());

        public Frame CreateFrame(TTextures texture, string sprite, Coord size)
        {
            var sprites = _frameLabels.Select(l => Sprites.TryGet(texture, $"{sprite}-{l}", out var s) ? s : null)
                .ToArray();
            return new Frame(Input, CurrentTileSize, sprites) {
                Size = size
            };
        }
    }
}
