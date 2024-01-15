using SFML.Graphics;

namespace Fiero.Business
{
    public partial class Animation
    {
        public class SpeechBubble(
            TimeSpan persistDuration,
            string text,
            int msPerChar = 24,
            ColorName textColor = ColorName.Black,
            ColorName bubbleColor = ColorName.White,
            TextureName font = TextureName.FontMonospace,
            bool invert = false
        )
        {
            public static SpeechBubble Alert => new(TimeSpan.FromSeconds(1), "!", 24, ColorName.White, ColorName.LightRed, font: TextureName.FontMonospace, invert: true);
            public static SpeechBubble Question => new(TimeSpan.FromSeconds(1), "?", 24, ColorName.White, ColorName.LightBlue, font: TextureName.FontMonospace, invert: true);

            private Animation cached;

            // Y offset of the entire speech bubble, in order to position it above an actor's head
            private const float SPEECH_Y = -0.33f;
            private readonly float variance = (float)Rng.Random.Between(-0.1, 0.1);
            // How many milliseconds should pass between each frame in the fadeout animation
            const int MS_PER_FADE = 4;
            // Base scale of the animation (TODO parametrize)
            private static readonly Vec s = new(0.5f, 0.5f);
            // Duration of the animation that types text one character at a time
            public readonly TimeSpan TypeAnimDuration = TimeSpan.FromMilliseconds(text.Length * msPerChar);
            // Total duration of the animation including the time that the bubble should persist
            public TimeSpan TotalDuration => TypeAnimDuration + persistDuration;
            // List of sprites representing the fully typed text
            private readonly SpriteDef[] textSprites = GetTextSprites(font, new Vec(0, SPEECH_Y), textColor, text, s / 2);
            public readonly int TotalFrames = (int)(text.Length + persistDuration.TotalMilliseconds / MS_PER_FADE);

            public event Action<SpeechBubble, char> CharDisplayed;

            // Generates the speech bubble frame and parametrizes its y value
            protected IEnumerable<SpriteDef> BackgroundSprites(float anim_y)
            {
                var ofs = new Vec(0, SPEECH_Y + anim_y);
                yield return new(TextureName.UI, "speech_bubble-l" + (invert ? "_inv" : ""), bubbleColor, ofs, s, 1, 0,
                    text.Length == 1 ? new IntRect(0, 0, 8, 16) : default);
                var isOdd = text.Length % 2 == 1;
                var k = Math.Floor((text.Length - 2) / 2f);
                for (int i = 0; i < k; i++)
                {
                    ofs += new Vec(s.X, 0);
                    yield return new(TextureName.UI, "speech_bubble-m" + (invert ? "_inv" : ""), bubbleColor, ofs, s, 1, 0);
                }
                ofs += new Vec(s.X, 0);
                if (text.Length == 1)
                    ofs -= new Vec(s.X / 2);
                if (isOdd && text.Length > 1)
                    yield return new(TextureName.UI, "speech_bubble-r_odd" + (invert ? "_inv" : ""), bubbleColor, ofs, s, 1, 0);
                else
                    yield return new(TextureName.UI, "speech_bubble-r" + (invert ? "_inv" : ""), bubbleColor, ofs, s, 1, 0);
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

            public Animation Animation => BakeAnimation();
            private Animation BakeAnimation()
            {
                if (cached != null)
                    return cached;
                // for the TypeAnimDuration we want each character to come out sequentially
                var progressiveWriteFrames = Enumerable.Range(0, text.Length)
                    .Select(i => new AnimationFrame(TimeSpan.FromMilliseconds(msPerChar),
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
                var anim = new Animation(progressiveWriteFrames
                    .Concat(fadeOutFrames)
                    .ToArray());
                anim.FramePlaying += Anim_FramePlaying;
                return cached = anim;
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

                void Anim_FramePlaying(Animation a, int i, AnimationFrame f)
                {
                    if (i < text.Length)
                        CharDisplayed?.Invoke(this, text[i]);
                }
            }
        }
    }
}
