namespace Fiero.Core
{
    public interface ISynthesizer

    {
        bool NextSample(int sr, float t, out double sample);
    }
}
