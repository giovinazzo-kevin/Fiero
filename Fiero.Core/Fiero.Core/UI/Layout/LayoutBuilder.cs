using LightInject;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public Layout Build(Coord size, Func<LayoutGrid, LayoutGrid> build)
        {
            var grid = build(new(size == Coord.Zero ? LayoutPoint.FromRelative(new(1, 1)) : LayoutPoint.FromAbsolute(size)));
            var controls = CreateRecursive(grid).ToArray();
            var layout = new Layout(grid, Input, controls);
            layout.Invalidated += _ =>
            {
                Console.WriteLine("inv");
                ResizeRecursive(layout.Size.V, layout.Position.V, grid);
            };
            ResizeRecursive(size, layout.Position.V, grid);
            return layout;

            Func<UIControl> GetResolver(Type controlType)
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

            IEnumerable<UIControl> CreateRecursive(LayoutGrid grid)
            {
                if (grid.IsCell)
                {
                    foreach (var c in grid.Controls)
                    {
                        var resolver = GetResolver(c.Type);
                        var control = resolver();
                        if (control != null)
                        {
                            c.Initialize?.Invoke(c.Instance = control);
                        }
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
            void ResizeRecursive(Coord screenSize, Coord offset, LayoutGrid grid, int i = 0)
            {
                if (screenSize == Coord.Zero)
                    return;
                // The outer grid should have (1,1) as its relative size. Therefore this line should change nothing if removed.
                // However leaving it lets users quickly verify the absolute size of their grids.
                grid.Size = LayoutPoint.FromAbsolute(screenSize);
                Inner(screenSize, grid, offset, i);
                void Inner(Coord screenSize, LayoutGrid grid, Coord p, int i = 0)
                {
                    // Get the total relative size of the children, in order to normalize them later
                    // NOTE: Rows have a relative size of 0 in their width, and cols have 0 for their height.
                    // Semantically, this means "auto". Therefore, these values are normalized by changing each 0 into a 1.
                    // What this means is that rows have 100% of the width of their parent, and columns have 100% of the height of their parent.
                    var totalRel = ApplyAutoSizing(grid.Select(x => x.Size.RelativePart).DefaultIfEmpty(new()).Aggregate((a, b) => a + b));
                    #region debugging
                    Console.Write(new string(' ', i));
                    var _x = grid.Size.RelativePart.X + grid.Size.AbsolutePart.X;
                    var _y = grid.Size.RelativePart.Y + grid.Size.AbsolutePart.Y;
                    if (_x == 0 && _y != 0)
                        Console.WriteLine($"ROW: {grid.Size.AbsolutePart}px + {grid.Size.RelativePart}*");
                    else if (_x != 0 && _y == 0)
                        Console.WriteLine($"COL: {grid.Size.AbsolutePart}px + {grid.Size.RelativePart}*");
                    else
                        Console.WriteLine($"GRD: {grid.Size.AbsolutePart}px + {grid.Size.RelativePart}*");
                    #endregion

                    foreach (var child in grid)
                    {
                        // Compute relative part of child position and normalize by totalRel
                        var rPos = child.Position.RelativePart / totalRel;
                        // Compute relative part of child size and normalize by totalRel, then apply auto sizing
                        var rSize = ApplyAutoSizing(child.Size.RelativePart / totalRel);
                        // Compute actual child position by summing global offset, child absolute position and calculated relative offset
                        var computedChildPos = (p + child.Position.AbsolutePart + rPos * screenSize).ToCoord();
                        // Compute actual child size by summing child absolute size and calculated relative size
                        var computedChildSize = (child.Size.AbsolutePart + rSize * screenSize).ToCoord();
                        Console.Write(new string(' ', i + 1));
                        Console.WriteLine($"- CHD: P{computedChildPos} S{computedChildSize}");
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

                Vec ApplyAutoSizing(Vec v)
                {
                    var (x, y) = v;
                    if (x == 0) x = 1f;
                    if (y == 0) y = 1f;
                    return new(x, y);
                }
            }
        }
    }
}
