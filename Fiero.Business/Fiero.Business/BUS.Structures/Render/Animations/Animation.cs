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
            TimeSpan? frameDuration = null,
            Coord? scale = null
        ) => new (
            Enumerable.Range(1, radius)
                .Select(i => new AnimationFrame(frameDuration ?? TimeSpan.FromMilliseconds(16), Shape.Circle(new(), i)
                    .Select(p => new AnimationSprite(texture, sprite, tint, p, scale ?? new(1, 1)))
                    .ToArray()))
                .ToArray()
        );

        public static Animation Flash(
            int durationInFrames,
            ColorName flashColor,
            string sprite = "Skull",
            TextureName texture = TextureName.Atlas,
            ColorName tint = ColorName.White,
            TimeSpan? frameDuration = null,
            Coord? scale = null
        ) => new(
            Enumerable.Range(1, durationInFrames)
                .Select(i => new AnimationFrame(
                    frameDuration ?? TimeSpan.FromMilliseconds(32), 
                    new AnimationSprite(texture, sprite, i % 2 == 0 ? flashColor : tint, new(), scale ?? new(1, 1))))
                .ToArray()
        );

        public static Animation Projectile(
            Coord from,
            Coord to,
            string sprite = "Skull",
            TextureName texture = TextureName.Atlas,
            ColorName tint = ColorName.White,
            TimeSpan? frameDuration = null,
            Coord? scale = null
        ) => new(
            Shape.Line(to, from)
                .Reverse()
                .Select(p => new AnimationFrame(
                    frameDuration ?? TimeSpan.FromMilliseconds(16), new AnimationSprite(texture, sprite, tint, p - from, scale ?? new(1, 1))))
                .ToArray()
        );

        public static Animation Explosion(
            ColorName tint = ColorName.LightYellow,
            TimeSpan? frameDuration = null,
            Coord? scale = null
        ) => new(
            Enumerable.Range(0, 8).Select(i => new AnimationFrame(
                frameDuration ?? TimeSpan.FromMilliseconds(16), 
                new AnimationSprite(TextureName.Animations, $"Explosion_{i+1}", tint, new(), scale ?? new(2, 2))))
            .ToArray()
        );
    }
}
