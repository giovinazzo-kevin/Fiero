namespace Fiero.Core
{
    public static class RandomNumberExtensions
    {
        public static bool CoinFlip(this IRandomNumber rng) => rng.Next() <= rng.Range / 2;
    }
}
