using System.Reflection;

namespace Fiero.Business
{
    public static class TileName
    {
        public const string None = nameof(None);
        public const string Room = nameof(Room);
        public const string Corridor = nameof(Corridor);
        public const string Shop = nameof(Shop);
        public const string Wall = nameof(Wall);
        public const string Water = nameof(Water);
        public const string Error = nameof(Error);

        public static readonly string[] _Values = typeof(TileName).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(x => x.Name != nameof(_Values))
            .Select(x => (string)x.GetValue(null))
            .ToArray();
    }
}
