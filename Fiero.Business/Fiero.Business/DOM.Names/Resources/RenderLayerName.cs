using System.Reflection;

namespace Fiero.Business
{
    public static class RenderLayerName
    {
        public const int Ground = 0;
        public const int Items = 1;
        public const int Features = 2;
        public const int BackgroundEffects = 3;
        public const int Wall = 4;
        public const int Actors = 5;
        public const int ForegroundEffects = 6;
        public const int UserInterface = 7;

        public static readonly int[] _Values = typeof(RenderLayerName).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(x => x.Name != nameof(_Values))
            .Select(x => (int)x.GetValue(null))
            .ToArray();
    }
}
