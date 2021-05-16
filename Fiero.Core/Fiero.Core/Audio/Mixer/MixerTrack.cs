using SFML.Audio;
using SFML.System;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;

namespace Fiero.Core
{

    /// <summary>
    /// Mixes incoming audio from other sources, including other mixer tracks.
    /// </summary>
    public class MixerTrack : ISynthesizer
    {
        protected readonly HashSet<ISynthesizer> Synths = new();
        public readonly List<IEffect> Effects = new();

        public int SampleRate { get; }

        public Knob<float> Volume { get; private set; }
        public Knob<bool> Mute { get; private set; }

        public MixerTrack(int sampleRate)
        {
            SampleRate = sampleRate;
            Volume = new(min: 0, max: 1, init: .5f, OnVolumeChanged);
            Mute = new(false, true, false, OnMuteChanged);
        }

        public bool Attach(ISynthesizer source)
        {
            Synths.Add(source);
            return true;
        }

        public bool Detach(ISynthesizer source)
        {
            Synths.Remove(source);
            return true;
        }

        protected virtual void OnMuteChanged(bool mute)
        {

        }

        protected virtual void OnVolumeChanged(float volume)
        {

        }

        public bool NextSample(int sr, float t, out double sample)
        {
            sample = 0;
            if (Mute)
                return true;
            foreach (var synth in Synths) {
                if (synth.NextSample(sr, t, out var synthSample)) {
                    sample += synthSample;
                }
            }
            foreach (var fx in Effects) {
                if(fx.NextSample(sr, t, sample, out var effectedSample)) {
                    sample = effectedSample;
                }
            }
            sample = Math.Clamp(Volume.V * sample, -1, 1);
            return true;
        }
    }
}
