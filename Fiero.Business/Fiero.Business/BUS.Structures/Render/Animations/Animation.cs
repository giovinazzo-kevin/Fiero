using Fiero.Core;
using SFML.Graphics;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Shapes = Fiero.Core.Shapes;

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
            Vec? scale = null
        ) => new(
            Enumerable.Range(1, radius)
                .Select(i => new AnimationFrame(frameDuration ?? TimeSpan.FromMilliseconds(24), Shapes.Circle(new(), i)
                    .Select(p => new AnimationSprite(texture, sprite, tint, p.ToVec(), scale ?? new(1, 1)))
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
            Vec? scale = null
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
            Vec? scale = null
        )
        {
            return new(
            Shapes.Line(to, from)
                .Reverse()
                .SelectMany(p => new AnimationFrame[]{
                    new(frameDuration ?? TimeSpan.FromMilliseconds(8), new AnimationSprite(texture, sprite, tint, (p - from).ToVec() + (from - p).ToVec().Clamp(-1, 1) / 2, scale ?? new(1, 1))),
                    new(frameDuration ?? TimeSpan.FromMilliseconds(8), new AnimationSprite(texture, sprite, tint, (p - from).ToVec(), scale ?? new(1, 1))),
                    new(frameDuration ?? TimeSpan.FromMilliseconds(8), new AnimationSprite(texture, sprite, tint, (p - from).ToVec() - (from - p).ToVec().Clamp(-1, 1) / 2, scale ?? new(1, 1))),
                })
                .ToArray());
        }

        public static Animation DamageNumber(
            int damage,
            ColorName tint = ColorName.White,
            Vec? scale = null
        )
        {
            var s = scale ?? new(0.5f, 0.5f);
            var str = damage.ToString();

            return new(
                new AnimationFrame(TimeSpan.FromMilliseconds(3 * 10), GetSprites(new(0, +0.00f), ColorName.White, str)),
                new AnimationFrame(TimeSpan.FromMilliseconds(3 * 12), GetSprites(new(0, -0.25f), tint, str)),
                new AnimationFrame(TimeSpan.FromMilliseconds(3 * 32), GetSprites(new(0, -0.50f), tint, str)),
                new AnimationFrame(TimeSpan.FromMilliseconds(3 * 16), GetSprites(new(0, -0.25f), ColorName.White, str)),
                new AnimationFrame(TimeSpan.FromMilliseconds(3 * 12), GetSprites(new(0, +0.00f), ColorName.White, str)),
                new AnimationFrame(TimeSpan.FromMilliseconds(3 * 10), GetSprites(new(0, +0.25f), tint, str)),
                new AnimationFrame(TimeSpan.FromMilliseconds(3 *  8), GetSprites(new(0, +0.50f), tint, str))
            );

            AnimationSprite[] GetSprites(Vec ofs, ColorName tint, string text)
            {
                return text.Select((c, i) => new AnimationSprite(
                        TextureName.FontLight, 
                        ((int)c).ToString(), 
                        tint, 
                        ofs + new Vec(i * s.X - text.Length * s.X / 4, 0),
                        s
                    ))
                    .ToArray();
            }
        }

        public static Animation Explosion(
            ColorName tint = ColorName.LightYellow,
            TimeSpan? frameDuration = null,
            Vec? scale = null
        ) => new(
            Enumerable.Range(0, 8).Select(i => new AnimationFrame(
                frameDuration ?? TimeSpan.FromMilliseconds(i < 6 ? i < 3 ? 8 : 24 : 48),
                new AnimationSprite(TextureName.Animations, $"Explosion_{i + 1}", tint, new(), scale ?? new(2, 2))))
            .ToArray()
        );
    }
}
