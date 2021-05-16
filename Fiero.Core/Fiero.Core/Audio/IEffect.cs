namespace Fiero.Core
{
    public interface IEffect
    {
        bool NextSample(int sr, float t, double sample, out double effectedSample);
    }
}
