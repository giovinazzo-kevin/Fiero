using SFML.Audio;
using SFML.System;

namespace Fiero.Core
{
    /// <summary>
    /// Contains several MixerTracks and a master track and buffers all incoming audio from the tracks.
    /// Also an instance of SoundStream and, as such, actually playable.
    /// </summary>
    public class Mixer : SoundStream
    {
        private ManualResetEvent BufferRefill = new(false);
        private Time _lastGetDataCall;
        private readonly MixerTrack[] _tracks;
        public IReadOnlyList<MixerTrack> Tracks => _tracks;
        public readonly MixerTrack Master;

        protected readonly Buffer Buffer;

        public Knob<bool> Unbuffered = new(false, true, false);

        public Mixer(int nTracks, int sampleRate)
        {
            Master = new(sampleRate);
            _tracks = new MixerTrack[nTracks];
            for (int i = 0; i < nTracks; i++)
            {
                _tracks[i] = new(sampleRate);
                Master.Attach(_tracks[i]);
            }
            Buffer = new(sampleRate);
            Buffer.Duration = Time.FromMilliseconds(100);
            for (int i = 0; i < Buffer.DurationInSamples; i++)
            {
                Buffer.Store(0);
            }
            Initialize(1, (uint)sampleRate);
            Task.Run(FillBuffer);
        }

        protected void FillBuffer()
        {
            while (true)
            {
                BufferRefill.WaitOne();
                if (!Unbuffered)
                {
                    while (!Buffer.Full)
                    {
                        var t = PlayingOffset.AsSeconds();
                        var d = Buffer.Duration.AsSeconds() / Buffer.DurationInSamples;
                        if (!Master.NextSample((int)SampleRate, t + Buffer.UsedDurationInSamples * d, out var sample))
                        {
                            break;
                        }
                        if (!Buffer.Store(Sample.Denormalize(sample)))
                        {
                            break;
                        }
                    }
                }
            }
        }

        protected override bool OnGetData(out short[] samples)
        {
            var delta = PlayingOffset - _lastGetDataCall;
            //Console.WriteLine(delta.AsMilliseconds());
            var ret = Unbuffered ? GetDataUnbuffered(out samples) : GetDataBuffered(out samples);
            _lastGetDataCall = PlayingOffset;
            return ret;

            bool GetDataUnbuffered(out short[] samples)
            {
                var freq = 0.02d;
                var requestSamples = (int)(SampleRate * freq); // 20ms is unfortunately the lowest achievable delay
                samples = new short[requestSamples];
                var t = PlayingOffset.AsSeconds();
                var w = freq / requestSamples;
                for (int i = 0; i < requestSamples; i++)
                {
                    if (!Master.NextSample((int)SampleRate, (float)(t + i * w), out var sample))
                    {
                        return false;
                    }
                    samples[i] = Sample.Denormalize(sample);
                }
                return true;
            }

            bool GetDataBuffered(out short[] samples)
            {
                var requestSamples = Buffer.DurationInSamples / 5;
                var ret = Buffer.Read(requestSamples, out samples) == requestSamples;
                BufferRefill.Set();
                return ret;
            }
        }

        protected override void OnSeek(Time timeOffset)
        {
        }
    }
}
