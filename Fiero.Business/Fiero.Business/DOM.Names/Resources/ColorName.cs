using System.Reflection;

namespace Fiero.Business
{
    public static class ColorName
    {
        public const string White = nameof(White);
        public const string Red = nameof(Red);
        public const string Green = nameof(Green);
        public const string Blue = nameof(Blue);
        public const string Cyan = nameof(Cyan);
        public const string Yellow = nameof(Yellow);
        public const string Magenta = nameof(Magenta);
        public const string Gray = nameof(Gray);
        public const string LightGray = nameof(LightGray);
        public const string LightRed = nameof(LightRed);
        public const string LightGreen = nameof(LightGreen);
        public const string LightBlue = nameof(LightBlue);
        public const string LightCyan = nameof(LightCyan);
        public const string LightYellow = nameof(LightYellow);
        public const string LightMagenta = nameof(LightMagenta);
        public const string Black = nameof(Black);
        public const string Transparent = nameof(Transparent);
        public const string UIPrimary = nameof(UIPrimary);
        public const string UISecondary = nameof(UISecondary);
        public const string UIAccent = nameof(UIAccent);
        public const string UIBorder = nameof(UIBorder);
        public const string UIDisabled = nameof(UIDisabled);
        public const string UIBackground = nameof(UIBackground);

        public static readonly string[] _Values = typeof(ColorName).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(x => x.Name != nameof(_Values))
            .Select(x => (string)x.GetValue(null))
            .ToArray();
    }
}
