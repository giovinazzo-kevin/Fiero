using Fiero.Core;
using SFML.Graphics;
using System;
using System.Linq;

namespace Fiero.Business
{
    public partial class Animation
    {
        public static Animation ExpandingRing(
            int radius, 
            string sprite = "Skull", 
            TextureName texture = TextureName.Atlas, 
            Color? tint = null,
            TimeSpan? frameDuration = null
        ) => new (
            Enumerable.Range(1, radius)
                .Select(i => new AnimationFrame(frameDuration ?? TimeSpan.FromMilliseconds(16), CoordEnumerable.Circle(i)
                    .Select(p => new AnimationSprite(texture, sprite, tint ?? Color.White, p))
                    .ToArray()))
                .ToArray()
        );

        public static Animation Flash(
            int durationInFrames,
            Color flashColor,
            string sprite = "Skull",
            TextureName texture = TextureName.Atlas,
            Color? tint = null,
            TimeSpan? frameDuration = null
        ) => new(
            Enumerable.Range(1, durationInFrames)
                .Select(i => new AnimationFrame(
                    frameDuration ?? TimeSpan.FromMilliseconds(32), new AnimationSprite(texture, sprite, i % 2 == 0 ? flashColor : tint ?? Color.White, new())))
                .ToArray()
        );

        public static Animation Projectile(
            Coord from,
            Coord to,
            string sprite = "Skull",
            TextureName texture = TextureName.Atlas,
            Color? tint = null,
            TimeSpan? frameDuration = null
        ) => new(
            Utils.BresenhamPoints(from, to)
                .Select(p => new AnimationFrame(
                    frameDuration ?? TimeSpan.FromMilliseconds(16), new AnimationSprite(texture, sprite, tint ?? Color.White, p - from)))
                .ToArray()
        );
    }
}
