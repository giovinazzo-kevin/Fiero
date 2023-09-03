namespace Fiero.Core
{
    public class LayoutBuilder
    {
        public readonly IServiceFactory ServiceProvider;
        public readonly GameInput Input;

        public LayoutBuilder(IServiceFactory serviceProvider)
        {
            ServiceProvider = serviceProvider;
            Input = ServiceProvider.GetInstance<GameInput>();
        }

        public Func<UIControl> GetResolver(Type controlType, LayoutGrid grid)
        {
            var resolverType = typeof(IUIControlResolver<>).MakeGenericType(controlType);
            var resolver = ServiceProvider.GetInstance(resolverType);
            var resolveMethod = resolver.GetType().GetMethod(nameof(IUIControlResolver<UIControl>.Resolve));
            return () =>
            {
                var control = resolveMethod.Invoke(resolver, new object[] { grid });
                return (UIControl)control;
            };
        }

        public Layout Build(Coord size, Func<LayoutGrid, LayoutGrid> build)
        {
            var grid = build(new(size == Coord.Zero ? LayoutPoint.FromRelative(new(1, 1)) : LayoutPoint.FromAbsolute(size)));
            var controls = CreateRecursive(grid).ToArray();
            var layout = new Layout(grid, Input, controls);
            layout.Position.ValueChanged += (_, old) =>
            {
                MoveRecursive(layout.Position.V - old, grid);
            };
            layout.Invalidated += _ =>
            {
                ResizeRecursive(layout.Size.V, layout.Position.V, grid);
            };
            if (size != Coord.Zero)
                ResizeRecursive(size, layout.Position.V, grid);
            return layout;

            IEnumerable<UIControl> CreateRecursive(LayoutGrid grid)
            {
                if (grid.IsCell)
                {
                    foreach (var c in grid.Controls)
                    {
                        var instance = c.Instance ?? GetResolver(c.Type, grid)();
                        if (c.Instance == null && instance != null)
                            c.Initialize?.Invoke(instance);
                        c.Instance = instance;
                    }
                }

                var enumerator = grid.GetEnumerator();
                for (int i = 0; i < grid.Cols + 1; i++)
                {
                    for (int j = 0; j < grid.Rows + 1; j++)
                    {
                        if (!enumerator.MoveNext())
                        {
                            yield break;
                        }
                        var child = enumerator.Current;
                        foreach (var inner in CreateRecursive(child))
                        {
                            yield return inner;
                        }
                        if (child.IsCell)
                        {
                            foreach (var c in child.Controls)
                            {
                                yield return c.Instance;
                            }
                        }
                    }
                }
            }

            void MoveRecursive(Coord delta, LayoutGrid grid, int i = 0)
            {
                Inner(delta, grid, i);
                void Inner(Coord delta, LayoutGrid grid, int i = 0)
                {
                    foreach (var child in grid)
                    {
                        if (child.IsCell)
                        {
                            foreach (var c in child.Controls)
                            {
                                c.Instance.Position.V += delta;
                            }
                        }
                        Inner(delta, child, i + 1);
                    }
                }
            }
            void ResizeRecursive(Coord screenSize, Coord offset, LayoutGrid grid, int i = 0)
            {
                if (screenSize == Coord.Zero)
                    return;
                // The outer grid should have (1,1) as its relative size. Therefore this line should change nothing if removed.
                // However leaving it lets users quickly check the absolute size of their top-level grids.
                grid.Size = LayoutPoint.FromAbsolute(screenSize);
                Inner(screenSize, grid, offset, i);
                void Inner(Coord screenSize, LayoutGrid grid, Coord p, int i = 0)
                {
                    // Get the total relative size of the children, in order to normalize them later
                    // NOTE: Rows have a relative size of 0 in their width, and cols have 0 for their height.
                    // Semantically, this means "auto". Therefore, these values are normalized by changing each 0 into a 1.
                    // What this means is that rows have 100% of the width of their parent, and columns have 100% of the height of their parent.
                    var totalRel = grid.Select(x => x.Size.RelativePart).DefaultIfEmpty(new()).Aggregate((a, b) => a + b);
                    totalRel = ApplyAutoSizing(LayoutPoint.FromRelative(totalRel));
                    // Get the total absolute value, in order to calculate how much space is left for the relative part
                    var totalAbs = grid.Select(x => x.Size.AbsolutePart).DefaultIfEmpty(new()).Aggregate((a, b) => a + b);
                    var unclaimedArea = screenSize - totalAbs;
                    foreach (var child in grid)
                    {
                        // Compute relative part of child position and normalize by totalRel
                        var rPos = child.Position.RelativePart / totalRel;
                        // Compute relative part of child size and normalize by totalRel, then apply auto sizing
                        var rSize = ApplyAutoSizing(child.Size / totalRel);
                        // Compute actual child position by summing global offset, child absolute position and calculated relative offset
                        var computedChildPos = child.ComputedPosition = (p + child.Position.AbsolutePart + (rPos * unclaimedArea).Floor()).ToCoord();
                        // Compute actual child size by summing child absolute size and calculated relative size
                        var computedChildSize = child.ComputedSize = (child.Size.AbsolutePart + (rSize * unclaimedArea).Ceiling()).ToCoord();
                        if (child.IsCell)
                        {
                            foreach (var c in child.Controls)
                            {
                                c.Instance.Position.V = computedChildPos;
                                c.Instance.Size.V = computedChildSize;
                                foreach (var rule in grid.GetStyles(c.Type))
                                {
                                    rule(c.Instance);
                                }
                            }
                        }
                        Inner(computedChildSize, child, computedChildPos, i + 1);
                    }
                }

                Vec ApplyAutoSizing(LayoutPoint p)
                {
                    var (x, y) = p.RelativePart;
                    if (x == 0) x = 1f;
                    if (y == 0) y = 1f;
                    if (p.AbsolutePart.X != 0) x = 0f;
                    if (p.AbsolutePart.Y != 0) y = 0f;
                    return new Vec(x, y);
                }
            }
        }
    }
}
