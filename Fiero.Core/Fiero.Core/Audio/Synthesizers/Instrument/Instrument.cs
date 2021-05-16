using SFML.System;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace Fiero.Core
{
    public class Instrument : ISynthesizer
    {
        private int _sampleRate = 44100;

        protected readonly Func<Oscillator> GetOscillator;

        protected readonly List<(Oscillator Osc, Envelope Env, Knob<int> Duration)> Sounds = new();
        public bool IsPlaying => Sounds.Count > 0;

        public Instrument(Func<Oscillator> getOsc)
        {
            GetOscillator = getOsc ?? (() => new Oscillator(OscillatorShape.Square));
        }

        public void Play(Note note, int octave, float durationInSeconds, float velocity = 1)
        {
            var osc = GetOscillator();
            var env = new Envelope();
            osc.Frequency.V = Oscillator.CalculateFrequency(note, octave);
            osc.Amplitude.V = velocity;
            var durationInSamples = (int)(durationInSeconds * _sampleRate);
            Sounds.Add((osc, env, new(0, durationInSamples, durationInSamples)));
            env.Engage();
        }

        public bool NextSample(int sr, float t, out double sample)
        {
            _sampleRate = sr;
            sample = 0;
            for (int i = Sounds.Count - 1; i >= 0; i--) {
                var (osc, env, dur) = Sounds[i];
                if(dur.V == 1) {
                    env.Disengage();
                }
                osc.NextSample(sr, t, out var oscSample);
                env.NextSample(sr, t, out var envelopeSample);
                sample += oscSample * envelopeSample;
                dur.V -= 1;
                if(env.State == EnvelopeState.Off) {
                    Sounds.RemoveAt(i);
                }
            }
            return true;
        }
    }
}
