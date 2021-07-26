using Fiero.Core;
using SFML.Graphics;
using System;
using System.Linq;
using Shape = Fiero.Core.Shape;

namespace Fiero.Business
{
    public partial class Animation
    {
        public static Animation ExpandingRing(
            int radius, 
            string sprite = "Skull", 
            TextureName texture = TextureName.Atlas,
            ColorName tint = ColorName.White,
            TimeSpan? frameDuration = null
        ) => new (
            Enumerable.Range(1, radius)
                .Select(i => new AnimationFrame(frameDuration ?? TimeSpan.FromMilliseconds(16), Shape.Circle(new(), i)
                    .Select(p => new AnimationSprite(texture, sprite, tint, p))
                    .ToArray()))
                .ToArray()
        );

        public static Animation Flash(
            int durationInFrames,
            ColorName flashColor,
            string sprite = "Skull",
            TextureName texture = TextureName.Atlas,
            ColorName tint = ColorName.White,
            TimeSpan? frameDuration = null
        ) => new(
            Enumerable.Range(1, durationInFrames)
                .Select(i => new AnimationFrame(
                    frameDuration ?? TimeSpan.FromMilliseconds(32), new AnimationSprite(texture, sprite, i % 2 == 0 ? flashColor : tint, new())))
                .ToArray()
        );

        public static Animation Projectile(
            Coord from,
            Coord to,
            string sprite = "Skull",
            TextureName texture = TextureName.Atlas,
            ColorName tint = ColorName.White,
            TimeSpan? frameDuration = null
        ) => new(
            Shape.Line(to, from)
                .Reverse()
                .Select(p => new AnimationFrame(
                    frameDuration ?? TimeSpan.FromMilliseconds(16), new AnimationSprite(texture, sprite, tint, p - from)))
                .ToArray()
        );
    }
}
