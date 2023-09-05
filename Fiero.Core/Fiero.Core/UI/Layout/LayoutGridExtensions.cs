using SFML.Graphics;

namespace Fiero.Core
{
    public static class LayoutGridExtensions
    {
        public static IntRect Inflate(this IntRect rect, int area)
        {
            return new(rect.Left - area, rect.Top - area, rect.Width + area, rect.Height + area);
        }

        public static LayoutGrid If(this LayoutGrid grid, bool condition, Func<LayoutGrid, LayoutGrid> apply)
        {
            if (condition)
            {
                return apply(grid);
            }
            return grid;
        }
    }
}
