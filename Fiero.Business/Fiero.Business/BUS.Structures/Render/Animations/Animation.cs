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
            string sprite = "Explosion_1",
            TextureName texture = TextureName.Animations,
            ColorName tint = ColorName.White,
            TimeSpan? frameDuration = null,
            Vec? scale = null
        ) => new(
            Enumerable.Range(1, radius)
                .Select(i => new AnimationFrame(frameDuration ?? TimeSpan.FromMilliseconds(24), Shapes.Circle(new(), i)
                    .Select(p => new SpriteDef(texture, sprite, tint, p.ToVec(), scale ?? new(1, 1)))
                    .ToArray()))
                .ToArray()
        );

        public static Animation Debuff(
            ColorName tint = ColorName.White,
            TimeSpan? frameDuration = null,
            Vec? scale = null
        )
        {
            return new(Enumerable.Range(0, 9).Select(i => new AnimationFrame(
                frameDuration ?? TimeSpan.FromMilliseconds(24),
                new SpriteDef(TextureName.Animations, $"Debuff_{i + 1}", tint, new(), scale ?? new(1, 1))))
            .ToArray());
        }

        public static Animation Buff(
            ColorName tint = ColorName.White,
            TimeSpan? frameDuration = null,
            Vec? scale = null
        )
        {
            return new(Enumerable.Range(0, 9).Select(i => new AnimationFrame(
                frameDuration ?? TimeSpan.FromMilliseconds(24),
                new SpriteDef(TextureName.Animations, $"Buff_{i + 1}", tint, new(), scale ?? new(1, 1))))
            .ToArray());
        }

        public static Animation StraightProjectile(
            Coord to,
            string sprite = "Rock",
            TextureName texture = TextureName.Items,
            ColorName tint = ColorName.White,
            Func<int, TimeSpan> frameDuration = null,
            Vec? scale = null,
            Coord offset = default
        )
        {
            var dir = to.ToVec().Clamp(-1, 1);
            var a = dir * 0.33f;
            frameDuration ??= (_ => TimeSpan.FromMilliseconds(10));
            return new(
            Shapes.Line(new(), to)
                .Skip(1).SkipLast(1)
                .SelectMany(p => new Vec[] { p - a, p.ToVec(), p + a })
                .Select((p, i) => new AnimationFrame(frameDuration(i), new SpriteDef(texture, sprite, tint, offset + p, scale ?? new(1, 1))))
                .ToArray());
        }

        public static Animation ArcingProjectile(
            Coord to,
            string sprite = "Rock",
            TextureName texture = TextureName.Items,
            ColorName tint = ColorName.White,
            Func<int, TimeSpan> frameDuration = null,
            Vec? scale = null,
            Coord offset = default
        )
        {
            var dir = to.ToVec().Clamp(-1, 1);
            var a = dir * 0.33f;
            frameDuration ??= (_ => TimeSpan.FromMilliseconds(30));
            var line = Shapes.Line(new(), to)
                .SelectMany(p => new Vec[] { p - a, p.ToVec(), p + a })
                .Skip(1).SkipLast(1)
                .ToArray();
            return new(
                line.Select((p, i) => {
                    var v = offset + p;
                    var t = Quadratic(0.66f / line.Length, i, 0, line.Length - 1);
                    v += new Vec(0, t);
                    return new AnimationFrame(frameDuration(i), new SpriteDef(texture, sprite, tint, v, scale ?? new(1, 1)));
                })
                .ToArray());

            float Quadratic(float k, float x, float x1, float x2) => k * (x - x1) * (x - x2);
        }

        public static Animation Death(
            Actor actor,
            TimeSpan? frameDuration = null
        )
        {
            var sprite = new SpriteDef(actor.Render.TextureName, actor.Render.SpriteName, actor.Render.Color, new(), new(1, 1));
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
            var k = new Vec(0.5f, 0.5f);
            return new(new[] {
                new AnimationFrame(TimeSpan.FromMilliseconds(4), MakeSprite(new())),
                new AnimationFrame(TimeSpan.FromMilliseconds(10), MakeSprite(k * new Vec(0.25f, 0.25f) * direction)),
                new AnimationFrame(TimeSpan.FromMilliseconds(15), MakeSprite(k * new Vec(0.50f, 0.50f) * direction)),
                new AnimationFrame(TimeSpan.FromMilliseconds(09), MakeSprite(k * new Vec(0.75f, 0.75f) * direction)),
                new AnimationFrame(TimeSpan.FromMilliseconds(09), MakeSprite(k * new Vec(1.00f, 1.00f) * direction)),
                new AnimationFrame(TimeSpan.FromMilliseconds(10), MakeSprite(k * new Vec(0.75f, 0.75f) * direction)),
                new AnimationFrame(TimeSpan.FromMilliseconds(15), MakeSprite(k * new Vec(0.50f, 0.50f) * direction)),
                new AnimationFrame(TimeSpan.FromMilliseconds(15), MakeSprite(k * new Vec(0.25f, 0.25f) * direction)),
                new AnimationFrame(TimeSpan.FromMilliseconds(4), MakeSprite(new())),
            });

            SpriteDef MakeSprite(Vec ofs) =>
                new(actor.Render.TextureName, actor.Render.SpriteName, actor.Render.Color, ofs, new(1, 1));
        }

        public static Animation TeleportOut(
            Actor actor
        )
            => StraightProjectile(
                new(0, -25),
                actor.Render.SpriteName,
                actor.Render.TextureName,
                actor.Render.Color,
                i => TimeSpan.FromMilliseconds(Math.Max(4, 36 - (i + 1) * 4))
            );

        public static Animation TeleportIn(
            Actor actor
        )
            => StraightProjectile(
                new(0, 25),
                actor.Render.SpriteName,
                actor.Render.TextureName,
                actor.Render.Color,
                i => TimeSpan.FromMilliseconds(Math.Max(4, 36 - (i + 1) * 4)),
                offset: new(0, -25)
            );

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

            SpriteDef[] GetSprites(Vec ofs, ColorName tint, string text)
            {
                return text.Select((c, i) => new SpriteDef(
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
            Vec? scale = null,
            Vec? offset = null
        ) => new(
            Enumerable.Range(0, 8).Select(i => new AnimationFrame(
                frameDuration ?? TimeSpan.FromMilliseconds(i < 6 ? i < 3 ? 8 : 24 : 48),
                new SpriteDef(TextureName.Animations, $"Explosion_{i + 1}", tint, offset ?? Vec.Zero, scale ?? new(1, 1))))
            .ToArray()
        );
    }
}
