using SFML.Audio;
using SFML.System;
using System;

namespace Fiero.Core
{
    public class Sampler : ISynthesizer
    {
        public Knob<bool> Loop { get; private set; }
        public Knob<float> PlaybackSpeed { get; private set; }

        public SoundBuffer AudioSample { get; set; }

        public Sampler()
        {

        }

        public bool NextSample(int sr, float t, out double sample)
        {
            sample = 0; 
            var denormalized = default(short);
            if (AudioSample is null)
                return false;
            var pos = (int)(AudioSample.SampleRate * t * PlaybackSpeed * AudioSample.ChannelCount);
            if (AudioSample.Samples.Length - pos <= AudioSample.ChannelCount - 1) {
                if (!Loop)
                    return false;
                pos %= AudioSample.Samples.Length;
            }
            if(AudioSample.ChannelCount == 2) {
                // Downmix by averaging (can cause phase issues)
                denormalized = (short)((AudioSample.Samples[pos] + AudioSample.Samples[pos + 1]) / 2);
            }
            else {
                denormalized = AudioSample.Samples[pos];
            }
            sample = Sample.Normalize(denormalized);
            return true;
        }
    }
}
