using Fiero.Core;
using SFML.Graphics;
using System;
using System.ComponentModel.DataAnnotations;
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

        public static Animation StraightProjectile(
            Coord from,
            Coord to,
            string sprite = "Rock",
            TextureName texture = TextureName.Atlas,
            ColorName tint = ColorName.White,
            TimeSpan? frameDuration = null,
            Vec? scale = null
        )
        {
            var dir = (from - to).ToVec().Clamp(-1, 1);
            var a = dir * 0.33f;
            var frameDur = frameDuration ?? TimeSpan.FromMilliseconds(10);
            return new(
            Shapes.Line(to, from)
                .Reverse()
                .SelectMany(p => new Vec[] { p - a, p.ToVec(), p + a })
                .Select(p => new AnimationFrame(frameDur, new AnimationSprite(texture, sprite, tint, p - from, scale ?? new(1, 1))))
                .ToArray());
        }

        public static Animation ArcingProjectile(
            Coord from,
            Coord to,
            string sprite = "Rock",
            TextureName texture = TextureName.Atlas,
            ColorName tint = ColorName.White,
            TimeSpan? frameDuration = null,
            Vec? scale = null
        )
        {
            var dir = (from - to).ToVec().Clamp(-1, 1);
            var a = dir * 0.33f;
            var frameDur = frameDuration ?? TimeSpan.FromMilliseconds(30);
            var line = Shapes.Line(to, from)
                .SelectMany(p => new Vec[] { p - a, p.ToVec(), p + a })
                .Skip(1).SkipLast(1)
                .Reverse()
                .ToArray();
            return new(
                line.Select((p, i) => {
                    var v = p - from;
                    var t = Quadratic(0.66f / line.Length, i, 0, line.Length - 1);
                    v += new Vec(0, t);
                    return new AnimationFrame(frameDur, new AnimationSprite(texture, sprite, tint, v, scale ?? new(1, 1)));
                })
                .ToArray());

            float Quadratic(float k, float x, float x1, float x2) => k * (x - x1) * (x - x2);
        }

        public static Animation Death(
            Actor actor,
            TimeSpan? frameDuration = null
        )
        {
            var sprite = new AnimationSprite(actor.Render.TextureName, actor.Render.SpriteName, actor.Render.Color, new(), new(1, 1));
            var frameDur = frameDuration ?? TimeSpan.FromMilliseconds(32);
            return new(Enumerable.Range(0, 6).SelectMany(i => new[] {
                new AnimationFrame(frameDur, sprite),
                new AnimationFrame(frameDur)
            }).ToArray());
        }

        public static Animation MeleeAttack(
            Actor actor,
            Coord direction
        )
        {
            var k = new Vec(1.0f, 1.0f);
            return new(new[] {
                new AnimationFrame(TimeSpan.FromMilliseconds(10), MakeSprite(k * new Vec(0.25f, 0.25f) * direction)),
                new AnimationFrame(TimeSpan.FromMilliseconds(15), MakeSprite(k * new Vec(0.50f, 0.50f) * direction)),
                new AnimationFrame(TimeSpan.FromMilliseconds(09), MakeSprite(k * new Vec(0.75f, 0.75f) * direction)),
                new AnimationFrame(TimeSpan.FromMilliseconds(09), MakeSprite(k * new Vec(1.00f, 1.00f) * direction)),
                new AnimationFrame(TimeSpan.FromMilliseconds(10), MakeSprite(k * new Vec(0.75f, 0.75f) * direction)),
                new AnimationFrame(TimeSpan.FromMilliseconds(15), MakeSprite(k * new Vec(0.50f, 0.50f) * direction)),
                new AnimationFrame(TimeSpan.FromMilliseconds(15), MakeSprite(k * new Vec(0.25f, 0.25f) * direction)),
            });

            AnimationSprite MakeSprite(Vec ofs) =>
                new(actor.Render.TextureName, actor.Render.SpriteName, actor.Render.Color, ofs, new(1, 1));
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
                new AnimationFrame(TimeSpan.FromMilliseconds(4 * 10), GetSprites(new(0, +0.00f), ColorName.White, str)),
                new AnimationFrame(TimeSpan.FromMilliseconds(4 * 12), GetSprites(new(0, -0.25f), tint, str)),
                new AnimationFrame(TimeSpan.FromMilliseconds(4 * 32), GetSprites(new(0, -0.50f), tint, str)),
                new AnimationFrame(TimeSpan.FromMilliseconds(4 * 16), GetSprites(new(0, -0.25f), ColorName.White, str)),
                new AnimationFrame(TimeSpan.FromMilliseconds(4 * 12), GetSprites(new(0, +0.00f), ColorName.White, str)),
                new AnimationFrame(TimeSpan.FromMilliseconds(4 * 10), GetSprites(new(0, +0.25f), tint, str)),
                new AnimationFrame(TimeSpan.FromMilliseconds(4 *  8), GetSprites(new(0, +0.50f), tint, str))
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
