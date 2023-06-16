namespace Fiero.Core
{
    public static class LayoutGridExtensions
    {
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
