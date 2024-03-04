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
                new SpriteDef(texture, sprite, tint, new(), scale ?? new(1, 1), (float)Math.Sin(i / (float)resolution * Math.PI) * 2 - 1)))
            .ToArray(), repeat);
        }
        static int GetAngleDegree(Coord target)
        {
            var n = (int)(270 - (Math.Atan2(-target.Y, target.X)) * 180 / Math.PI) + 90;
            return n % 360;
        }

        public static Animation StraightProjectile(
            Coord to,
            string sprite = "Rock",
            TextureName texture = TextureName.Items,
            ColorName tint = ColorName.White,
            Func<int, TimeSpan> frameDuration = null,
            Vec? scale = null,
            Vec offset = default,
            int repeat = 0,
            bool directional = false,
            string trailSprite = null
        )
        {
            var dir = to.ToVec().Clamp(-1, 1);
            var a = dir * 0.33f;
            var b = dir * 0.5f;
            frameDuration ??= (_ => TimeSpan.FromMilliseconds(15));
            var rotation = 0;
            if (directional)
                rotation = GetAngleDegree(to);
            return new(
            Shapes.Line(new(), to)
                .Skip(1)
                .SelectMany(p => new Vec[] { p - a, p.ToVec(), p + a })
                .Select((p, i) =>
                {
                    var projSprite = new SpriteDef(texture, sprite, tint, offset + p, scale ?? new(1, 1), 1, Z: 1, Rotation: rotation);
                    SpriteDef[] trailSprites = [];
                    if (trailSprite != null)
                    {
                        trailSprites = Shapes.Line(new(), p.ToCoord())
                            .Skip(1)
                            .SelectMany(p => new Vec[] { p - b, p.ToVec() })
                            .Select(q => new SpriteDef(texture, trailSprite, tint, offset + q, scale ?? new(1, 1), 1, Z: 0, Rotation: rotation))
                            .ToArray();
                    }
                    return new AnimationFrame(frameDuration(i), [projSprite, .. trailSprites]);
                })
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
            int repeat = 0,
            bool directional = false,
            string trailSprite = null
        )
        {
            var dir = to.ToVec().Clamp(-1, 1);
            var a = dir * 0.33f;
            frameDuration ??= (_ => TimeSpan.FromMilliseconds(30));
            var rotation = 0;
            if (directional)
                rotation = GetAngleDegree(to);
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
                    return new AnimationFrame(frameDuration(i), new SpriteDef(texture, sprite, tint, v, scale ?? new(1, 1), 1, Rotation: rotation));
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

        public static Animation LevelUp(
            Actor actor,
            TimeSpan? frameDuration = null,
            int repeat = 0,
            ColorName tint = ColorName.LightYellow
        )
        {
            var sprite = new SpriteDef(actor.Render.Texture, actor.Render.Sprite, actor.Render.Color, actor.ActorProperties.Type != ActorName.None ? new Vec(0f, -0.166f) : Vec.Zero, new(1, 1), 1);
            var frameDur = frameDuration ?? TimeSpan.FromMilliseconds(48);
            return new(Enumerable.Range(0, 8).SelectMany(i => new[] {
                new AnimationFrame(frameDur, sprite),
                new AnimationFrame(frameDur, sprite with {Scale = new(2, 2), Tint = tint})
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
                new AnimationFrame(TimeSpan.FromMilliseconds(2 * 10), GetTextSprites(font, new(startX, +0.00f), tint, str, s)),
                new AnimationFrame(TimeSpan.FromMilliseconds(2 * 12), GetTextSprites(font, new(AnimateX(), -0.25f), tint, str, s)),
                new AnimationFrame(TimeSpan.FromMilliseconds(2 * 12), GetTextSprites(font, new(AnimateX(), -0.33f), tint, str, s)),
                new AnimationFrame(TimeSpan.FromMilliseconds(2 * 48), GetTextSprites(font, new(AnimateX(), -0.50f), tint, str, s)),
                new AnimationFrame(TimeSpan.FromMilliseconds(2 * 12), GetTextSprites(font, new(AnimateX(), -0.25f), tint, str, s)),
                new AnimationFrame(TimeSpan.FromMilliseconds(2 * 12), GetTextSprites(font, new(AnimateX(), +0.00f), tint, str, s)),
                new AnimationFrame(TimeSpan.FromMilliseconds(2 * 10), GetTextSprites(font, new(AnimateX(), +0.25f), tint, str, s)),
                new AnimationFrame(TimeSpan.FromMilliseconds(2 * 8), GetTextSprites(font, new(AnimateX(), +0.50f), tint, str, s))
            }, repeat);

            float AnimateX()
            {
                return startX;
            }
        }

        static SpriteDef[] GetTextSprites(TextureName font, Vec ofs, ColorName tint, string text, Vec s)
        {
            return text.Select((c, i) => new SpriteDef(
                    font,
                    ((int)c).ToString(),
                    tint,
                    ofs + new Vec(i * s.X, 0),
                    s,
                    1,
                    Z: 1
                ))
                .ToArray();
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
