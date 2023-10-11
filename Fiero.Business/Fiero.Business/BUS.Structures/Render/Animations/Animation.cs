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
            Vec? scale = null,
            int repeat = 0
        ) => new(
            Enumerable.Range(1, radius)
                .Select(i => new AnimationFrame(frameDuration ?? TimeSpan.FromMilliseconds(48), Shapes.Circle(new(), i)
                    .Select(p => new SpriteDef(texture, sprite, tint, p.ToVec(), scale ?? new(1, 1), 1))
                    .ToArray()))
                .ToArray(),
            repeat
        );

        public static Animation Debuff(
            ColorName tint = ColorName.White,
            TimeSpan? frameDuration = null,
            Vec? scale = null,
            int repeat = 0
        )
        {
            return new(Enumerable.Range(0, 9).Select(i => new AnimationFrame(
                frameDuration ?? TimeSpan.FromMilliseconds(24),
                new SpriteDef(TextureName.Animations, $"Debuff_{i + 1}", tint, new(), scale ?? new(1, 1), 1)))
            .ToArray(), repeat);
        }

        public static Animation Buff(
            ColorName tint = ColorName.White,
            TimeSpan? frameDuration = null,
            Vec? scale = null,
            int repeat = 0
        )
        {
            return new(Enumerable.Range(0, 9).Select(i => new AnimationFrame(
                frameDuration ?? TimeSpan.FromMilliseconds(24),
                new SpriteDef(TextureName.Animations, $"Buff_{i + 1}", tint, new(), scale ?? new(1, 1), 1)))
            .ToArray(), repeat);
        }

        public static Animation Fade(
            string sprite = "Rock",
            TextureName texture = TextureName.Items,
            ColorName tint = ColorName.White,
            TimeSpan? frameDuration = null,
            Vec? scale = null,
            int repeat = 0,
            int resolution = 10
        )
        {
            return new(Enumerable.Range(0, resolution).Select(i => new AnimationFrame(
                frameDuration ?? TimeSpan.FromMilliseconds(1000f / resolution),
                new SpriteDef(texture, sprite, tint, new(), scale ?? new(1, 1), 1 - i / (float)resolution)))
            .ToArray(), repeat);
        }

        public static Animation StraightProjectile(
            Coord to,
            string sprite = "Rock",
            TextureName texture = TextureName.Items,
            ColorName tint = ColorName.White,
            Func<int, TimeSpan> frameDuration = null,
            Vec? scale = null,
            Vec offset = default,
            int repeat = 0
        )
        {
            var dir = to.ToVec().Clamp(-1, 1);
            var a = dir * 0.33f;
            frameDuration ??= (_ => TimeSpan.FromMilliseconds(10));
            return new(
            Shapes.Line(new(), to)
                .Skip(1)
                .SelectMany(p => new Vec[] { p - a, p.ToVec(), p + a })
                .Select((p, i) => new AnimationFrame(frameDuration(i), new SpriteDef(texture, sprite, tint, offset + p, scale ?? new(1, 1), 1)))
                .ToArray(), repeat);
        }

        public static Animation ArcingProjectile(
            Coord to,
            string sprite = "Rock",
            TextureName texture = TextureName.Items,
            ColorName tint = ColorName.White,
            Func<int, TimeSpan> frameDuration = null,
            Vec? scale = null,
            Coord offset = default,
            int repeat = 0
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
                line.Select((p, i) =>
                {
                    var v = offset + p;
                    var t = Quadratic(0.66f / line.Length, i, 0, line.Length - 1);
                    v += new Vec(0, t);
                    return new AnimationFrame(frameDuration(i), new SpriteDef(texture, sprite, tint, v, scale ?? new(1, 1), 1));
                })
                .ToArray(), repeat);

            float Quadratic(float k, float x, float x1, float x2) => k * (x - x1) * (x - x2);
        }

        public static Animation Death(
            Actor actor,
            TimeSpan? frameDuration = null,
            int repeat = 0
        )
        {
            var sprite = new SpriteDef(actor.Render.Texture, actor.Render.Sprite, actor.Render.Color, actor.ActorProperties.Type != ActorName.None ? new Vec(0f, -0.166f) : Vec.Zero, new(1, 1), 1);
            var frameDur = frameDuration ?? TimeSpan.FromMilliseconds(32);
            return new(Enumerable.Range(0, 6).SelectMany(i => new[] {
                new AnimationFrame(frameDur, sprite),
                new AnimationFrame(frameDur)
            }).ToArray(), repeat);
        }

        public static Animation MeleeAttack(
            Actor actor,
            Coord direction,
            int repeat = 0
        )
        {
            var k = new Vec(0.5f, 0.5f);
            return new(new[] {
                new AnimationFrame(TimeSpan.FromMilliseconds(2 * 04), MakeSprite(new())),
                new AnimationFrame(TimeSpan.FromMilliseconds(2 * 10), MakeSprite(k * new Vec(0.25f, 0.25f) * direction)),
                new AnimationFrame(TimeSpan.FromMilliseconds(2 * 15), MakeSprite(k * new Vec(0.50f, 0.50f) * direction)),
                new AnimationFrame(TimeSpan.FromMilliseconds(2 * 09), MakeSprite(k * new Vec(0.75f, 0.75f) * direction)),
                new AnimationFrame(TimeSpan.FromMilliseconds(2 * 09), MakeSprite(k * new Vec(1.00f, 1.00f) * direction)),
                new AnimationFrame(TimeSpan.FromMilliseconds(2 * 10), MakeSprite(k * new Vec(0.75f, 0.75f) * direction)),
                new AnimationFrame(TimeSpan.FromMilliseconds(2 * 15), MakeSprite(k * new Vec(0.50f, 0.50f) * direction)),
                new AnimationFrame(TimeSpan.FromMilliseconds(2 * 15), MakeSprite(k * new Vec(0.25f, 0.25f) * direction)),
                new AnimationFrame(TimeSpan.FromMilliseconds(2 * 04), MakeSprite(new())),
            }, repeat);

            SpriteDef MakeSprite(Vec ofs) =>
                new(actor.Render.Texture, actor.Render.Sprite, actor.Render.Color, ofs - new Vec(0f, 0.166f), new(1, 1), 1);
        }

        public static Animation Wait(
            TimeSpan duration,
            int repeat = 0
        )
        {
            return new(new[] {
                new AnimationFrame(duration),
            }, repeat);
        }

        public static Animation TeleportOut(
            Actor actor,
            int repeat = 0
        )
            => StraightProjectile(
                new Coord(0, -25),
                actor.Render.Sprite,
                actor.Render.Texture,
                actor.Render.Color,
                i => TimeSpan.FromMilliseconds(10),
                offset: new Vec(0f, -0.166f),
                repeat: repeat
            );

        public static Animation TeleportIn(
            Actor actor,
            int repeat = 0
        )
            => StraightProjectile(
                new(0, 25),
                actor.Render.Sprite,
                actor.Render.Texture,
                actor.Render.Color,
                i => TimeSpan.FromMilliseconds(10),
                offset: new(0, -25 + 0.166f),
                repeat: repeat
            );

        public static Animation DamageNumber(
            int damage,
            TextureName font = TextureName.FontMonospace,
            ColorName tint = ColorName.White,
            Vec? scale = null,
            int repeat = 0
        )
        {
            var s = scale ?? new(0.5f, 0.5f);
            var str = damage.ToString();

            var startX = 0.25f;
            return new(new[] {
                new AnimationFrame(TimeSpan.FromMilliseconds(2 * 10), GetSprites(new(startX, +0.00f), tint, str)),
                new AnimationFrame(TimeSpan.FromMilliseconds(2 * 12), GetSprites(new(AnimateX(), -0.25f), tint, str)),
                new AnimationFrame(TimeSpan.FromMilliseconds(2 * 12), GetSprites(new(AnimateX(), -0.33f), tint, str)),
                new AnimationFrame(TimeSpan.FromMilliseconds(2 * 48), GetSprites(new(AnimateX(), -0.50f), tint, str)),
                new AnimationFrame(TimeSpan.FromMilliseconds(2 * 12), GetSprites(new(AnimateX(), -0.25f), tint, str)),
                new AnimationFrame(TimeSpan.FromMilliseconds(2 * 12), GetSprites(new(AnimateX(), +0.00f), tint, str)),
                new AnimationFrame(TimeSpan.FromMilliseconds(2 * 10), GetSprites(new(AnimateX(), +0.25f), tint, str)),
                new AnimationFrame(TimeSpan.FromMilliseconds(2 * 8), GetSprites(new(AnimateX(), +0.50f), tint, str))
            }, repeat);

            float AnimateX()
            {
                return startX;
            }

            SpriteDef[] GetSprites(Vec ofs, ColorName tint, string text)
            {
                return text.Select((c, i) => new SpriteDef(
                        font,
                        ((int)c).ToString(),
                        tint,
                        ofs + new Vec(i * s.X - text.Length * s.X / 4, 0),
                        s,
                        1
                    ))
                    .ToArray();
            }
        }

        public static Animation Explosion(
            ColorName tint = ColorName.LightYellow,
            TimeSpan? frameDuration = null,
            Vec? scale = null,
            Vec? offset = null,
            int repeat = 0
        ) => new(
            Enumerable.Range(0, 6).Select(i => new AnimationFrame(
                frameDuration ?? TimeSpan.FromMilliseconds(48),
                new SpriteDef(TextureName.Animations, $"Explosion_{i + 1}", tint, offset ?? Vec.Zero, scale ?? new(1, 1), 1)))
            .ToArray(), repeat
        );
    }
}
