using SFML.Audio;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Core
{
    /// <summary>
    /// Contains several MixerTracks and a master track and buffers all incoming audio from the tracks.
    /// Also an instance of SoundStream and, as such, actually playable.
    /// </summary>
    public class Mixer : SoundStream
    {
        private readonly MixerTrack[] _tracks;
        public IReadOnlyList<MixerTrack> Tracks => _tracks;
        public readonly MixerTrack Master;

        protected readonly Buffer Buffer;

        public Knob<bool> Unbuffered = new(false, true, false);

        public Mixer(int nTracks, int sampleRate)
        {
            Master = new(sampleRate);
            _tracks = new MixerTrack[nTracks];
            for (int i = 0; i < nTracks; i++) {
                _tracks[i] = new(sampleRate);
                Master.Attach(_tracks[i]);
            }
            Buffer = new(sampleRate);
            Buffer.Duration = Time.FromMilliseconds(80);
            Initialize(1, (uint)sampleRate);
        }

        protected override bool OnGetData(out short[] samples)
        {
            if(Unbuffered) {
                return GetDataUnbuffered(out samples);
            }
            return GetDataBuffered(out samples);

            bool GetDataUnbuffered(out short[] samples)
            {
                var requestSamples = (int)(SampleRate * 0.02f); // 20ms is sadly the lowest achievable delay
                samples = new short[requestSamples];
                var t = PlayingOffset.AsSeconds();
                var w = 0.02f / requestSamples;
                for (int i = 0; i < requestSamples; i++) {
                    if (!Master.NextSample((int)SampleRate, t + i * w, out var sample)) {
                        return false;
                    }
                    samples[i] = Sample.Denormalize(sample);
                }
                return true;
            }

            bool GetDataBuffered(out short[] samples)
            {
                var requestSamples = Buffer.DurationInSamples / 4;
                samples = new short[requestSamples];
                var t = PlayingOffset.AsSeconds();
                var w = Buffer.Duration.AsSeconds() / Buffer.DurationInSamples;
                if (!Buffer.Full) {
                    for (int i = 0; i < requestSamples; i++) {
                        if (!Master.NextSample((int)SampleRate, t + Buffer.UsedDurationInSamples * w, out var sample)) {
                            return false;
                        }
                        if (!Buffer.Store(Sample.Denormalize(sample))) {
                            break;
                        }
                    }
                }
                if (Buffer.Full) {
                    Buffer.Read(requestSamples, out samples);
                }
                return true;
            }
        }

        protected override void OnSeek(Time timeOffset)
        {
        }
    }
}
