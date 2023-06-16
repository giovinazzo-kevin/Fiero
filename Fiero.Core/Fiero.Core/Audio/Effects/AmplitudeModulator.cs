namespace Fiero.Core
{
    public class AmplitudeModulator : IEffect
    {
        public readonly Knob<float> Frequency = new(0.01f, 20000, 10);

        public AmplitudeModulator()
        {
        }

        public bool NextSample(int sr, float t, double sample, out double effectedSample)
        {
            effectedSample = sample * Math.Sin(t * Frequency);
            return true;
        }
    }
}
