namespace Fiero.Core
{
    public static class Sample
    {
        public static short Denormalize(double sample)
        {
            return (short)(Math.Clamp(sample, -1, 1) * short.MaxValue);
        }
        public static double Normalize(short sample)
        {
            return Math.Clamp(sample, (short)(-short.MaxValue), short.MaxValue) / short.MaxValue;
        }
    }
}
