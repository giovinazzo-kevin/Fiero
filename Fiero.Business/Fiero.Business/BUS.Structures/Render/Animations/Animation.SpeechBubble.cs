namespace Fiero.Business
{
    public partial class Animation
    {
        public class SpeechBubble(TimeSpan persistDuration, string text)
        {
            // Y offset of the entire speech bubble, in order to position it above an actor's head
            private const float SPEECH_Y = -0.33f;
            private readonly float variance = (float)Rng.Random.Between(-0.1, 0.1);
            // How many milliseconds the animation for each character's bounce should last
            const int MS_PER_CHAR = 24;
            // How many milliseconds should pass between each frame in the fadeout animation
            const int MS_PER_FADE = 4;
            // Base scale of the animation (TODO parametrize)
            private static readonly Vec s = new(0.5f, 0.5f);
            // Duration of the animation that types text one character at a time
            public readonly TimeSpan TypeAnimDuration = TimeSpan.FromMilliseconds(text.Length * MS_PER_CHAR);
            // Total duration of the animation including the time that the bubble should persist
            public TimeSpan TotalDuration => TypeAnimDuration + persistDuration;
            // List of sprites representing the fully typed text
            private readonly SpriteDef[] textSprites = GetTextSprites(TextureName.FontMonospace, new Vec(0, SPEECH_Y), ColorName.Black, text, s / 2);
            public readonly int TotalFrames = (int)(text.Length + persistDuration.TotalMilliseconds / MS_PER_FADE);

            // Generates the speech bubble frame and parametrizes its y value
            protected IEnumerable<SpriteDef> BackgroundSprites(float anim_y)
            {
                var ofs = new Vec(0, SPEECH_Y + anim_y);
                yield return new(TextureName.UI, "speech_bubble-l", ColorName.White, ofs, s, 1, 0);
                var isOdd = text.Length % 2 == 1;
                var k = Math.Floor((text.Length - 2) / 2f);
                for (int i = 0; i < k; i++)
                {
                    ofs += new Vec(s.X, 0);
                    yield return new(TextureName.UI, "speech_bubble-m", ColorName.White, ofs, s, 1, 0);
                }
                ofs += new Vec(s.X, 0);
                if (isOdd)
                    yield return new(TextureName.UI, "speech_bubble-r_odd", ColorName.White, ofs, s, 1, 0);
                else
                    yield return new(TextureName.UI, "speech_bubble-r", ColorName.White, ofs, s, 1, 0);
            }

            protected IEnumerable<SpriteDef> PartialText(float anim_y, int currentChar)
            {
                SpriteDef[] newSprites = [.. textSprites.Take(currentChar)];
                for (int i = 0; i < newSprites.Length; i++)
                {
                    ref var sprite = ref newSprites[i];
                    sprite = sprite with
                    {
                        Offset = new Vec(sprite.Offset.X, SPEECH_Y + anim_y),

                    };
                }
                return newSprites;
            }

            public Animation Animation()
            {
                // for the TypeAnimDuration we want each character to come out sequentially
                var progressiveWriteFrames = Enumerable.Range(0, text.Length)
                    .Select(i => new AnimationFrame(TimeSpan.FromMilliseconds(MS_PER_CHAR),
                        BackgroundSprites(anim_y: Y(i))
                        .Concat(PartialText(anim_y: Y(i), i))
                        .ToArray()));
                // For the persistDuration we want a smooth fade out of both the text and the speech bubble
                var numFadeFrames = (persistDuration.TotalMilliseconds / MS_PER_FADE);
                var alphaDt = 255f / numFadeFrames;
                var fadeOutFrames = Enumerable.Range(0, (int)numFadeFrames)
                    .Select(i => new AnimationFrame(TimeSpan.FromMilliseconds(MS_PER_FADE),
                        BackgroundSprites(anim_y: Y(i + text.Length))
                        .Select((s, j) => s with { Alpha = (float)(alphaDt * (1 - i / numFadeFrames)) })
                        .Concat(PartialText(anim_y: Y(i + text.Length), text.Length)
                            .Select((s, j) => s with { Alpha = (float)(alphaDt * (1 - i / numFadeFrames)) }))
                        .ToArray()));
                return new Animation(progressiveWriteFrames
                    .Concat(fadeOutFrames)
                    .ToArray());
                float Y(int i)
                {
                    var f = i / (float)TotalFrames;
                    var wobble = (float)ease(f * 5);
                    return wobble * 0.1f + variance;
                    double ease(float x)
                    {
                        const double c4 = (2 * Math.PI) / 3;
                        return x == 0
                          ? 0
                          : x == 1
                          ? 1
                          : Math.Pow(2, -10 * x) * Math.Sin((x * 10 - 0.75) * c4) + 1;
                    }
                }
            }
        }
    }
}
