using Fiero.Core;
using SFML.Graphics;
using SFML.System;
using System;

namespace Fiero.Business
{
    public static class UIExtensions
    {
        public static LayoutBuilder<FontName, TextureName, SoundName> ActorDialogue(
            this LayoutBuilder<FontName, TextureName, SoundName> builder, 
            Coord position, 
            Coord size, 
            Color? activeColor = null, 
            Color? inactiveColor = null, 
            string textSpriteName = "conversation", 
            Action<ActorDialogue> initialize = null)
        {
            var (ts, tex, font) = (builder.CurrentTileSize, builder.CurrentTexture, builder.CurrentFont);
            builder.Controls.Add(() => {
                var textFrame = builder.CreateFrame(tex, textSpriteName, size * ts);
                textFrame.ActiveColor = activeColor ?? Color.White;
                textFrame.InactiveColor = inactiveColor ?? new Color(192, 192, 192, 255);
                var dialogue = new ActorDialogue(builder.Input, 
                        s => builder.Sounds.Get(s), textFrame, 
                        sz => builder.CreateFrame(tex, textSpriteName, sz),
                        size.X, size.Y, ts, 
                        s => new Text(s, builder.Fonts.Get(font), (uint)ts * 2),
                        s => builder.Sprites.TryGet(tex, s, out var sprite) ? sprite : null) {
                    Position = new(position.X * ts, position.Y * ts),
                    Size = new(size.X * ts, size.Y * ts),
                    ActiveColor = textFrame.ActiveColor,
                    InactiveColor = textFrame.InactiveColor
                };
                initialize?.Invoke(dialogue);
                return dialogue;
            });
            return builder;
        }
    }
}
