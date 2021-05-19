using SFML.System;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;

namespace Fiero.Core
{
    public class Instrument : ISynthesizer
    {
        private int _sampleRate = 44100;

        protected readonly Func<Oscillator> GetOscillator;

        protected readonly ConcurrentQueue<(Oscillator Osc, Envelope Env, int Duration)> Sounds = new();
        public bool IsPlaying => Sounds.Count > 0;

        public Knob<int> MaxVoices = new(1, 16, 16);

        public Instrument(Func<Oscillator> getOsc)
        {
            GetOscillator = getOsc ?? (() => new Oscillator(OscillatorShape.Square));
        }

        public void Play(Note note, int octave, float durationInSeconds, float volume = 1)
        {
            if (Sounds.Count >= MaxVoices) {
                Sounds.TryDequeue(out _);
            }
            var osc = GetOscillator();
            var env = new Envelope();
            osc.Frequency.V = Oscillator.CalculateFrequency(note, octave);
            osc.Amplitude.V = volume;
            var durationInSamples = (int)(durationInSeconds * _sampleRate);
            Sounds.Enqueue((osc, env, durationInSamples));
            env.Engage();
        }

        public void StopAll()
        {
            Sounds.Clear();
        }

        public bool NextSample(int sr, float t, out double sample)
        {
            _sampleRate = sr;
            sample = 0;
            for (int i = 0; i < Sounds.Count; i++) {
                if(Sounds.TryDequeue(out var sound)) {
                    var (osc, env, dur) = sound;
                    if (dur == 1) {
                        env.Disengage();
                    }
                    osc.NextSample(sr, t, out var oscSample);
                    env.NextSample(sr, t, out var envelopeSample);
                    sample += oscSample * envelopeSample;
                    dur -= 1;
                    if (env.State != EnvelopeState.Off) {
                        Sounds.Enqueue((osc, env, dur));
                    }
                }

            }
            return true;
        }
    }
}
