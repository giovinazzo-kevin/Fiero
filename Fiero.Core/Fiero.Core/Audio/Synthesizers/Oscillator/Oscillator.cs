namespace Fiero.Core
{
    public class Oscillator : ISynthesizer
    {
        public Knob<float> Amplitude { get; private set; }
        public Knob<float> Frequency { get; private set; }
        public OscillatorShape Shape { get; set; }

        public Oscillator(OscillatorShape shape, float defaultAmplitude = 1, float defaultFrequency = 440)
        {
            Shape = shape;
            Amplitude = new(min: 0, max: 1, defaultAmplitude, OnVolumeChanged);
            Frequency = new(min: 20, max: 20000, defaultFrequency, OnFrequencyChanged);
        }

        protected virtual void OnVolumeChanged(float volume)
        {
        }

        protected virtual void OnFrequencyChanged(float freq)
        {

        }

        protected virtual void OnPhaseChanged(float phase)
        {

        }

        public static float CalculateFrequency(Note n, int octave)
        {
            const float C0 = 16.35f;
            const float A = 1.059463094359f;
            return C0 * (float)Math.Pow(A, (int)n + 12 * octave);
        }

        public bool NextSample(int sr, float time, out double sample)
        {
            sample = Shape switch
            {
                OscillatorShape.Sine => Wave.Sine(time, Amplitude.V, Frequency.V),
                OscillatorShape.Triangle => Wave.Triangle(time, Amplitude.V, Frequency.V),
                OscillatorShape.Saw => Wave.Saw(time, Amplitude.V, Frequency.V),
                OscillatorShape.Square => Wave.Square(time, Amplitude.V, Frequency.V),
                _ => 0
            };
            return true;
        }
    }
}
