using System.ComponentModel;

namespace Fiero.Business
{
    public enum ColorName
    {
        [Description("white")]
        White,
        [Description("red")]
        Red,
        [Description("green")]
        Green,
        [Description("blue")]
        Blue,
        [Description("cyan")]
        Cyan,
        [Description("yellow")]
        Yellow,
        [Description("magenta")]
        Magenta,
        [Description("gray")]
        Gray,
        [Description("light gray")]
        LightGray,
        [Description("light red")]
        LightRed,
        [Description("light green")]
        LightGreen,
        [Description("light blue")]
        LightBlue,
        [Description("light cyan")]
        LightCyan,
        [Description("light yellow")]
        LightYellow,
        [Description("light magenta")]
        LightMagenta,
        [Description("black")]
        Black,

        [Description("primary UI color")]
        UIPrimary,
        [Description("secondary UI color")]
        UISecondary,
        [Description("UI accent color")]
        UIAccent,
        [Description("UI background color")]
        UIBackground
    }
}
