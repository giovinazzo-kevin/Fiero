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
            var s = new Vec(1, 1);
            var grid = build(new(size == Coord.Zero ? LayoutPoint.FromRelative(new(1, 1)) : LayoutPoint.FromAbsolute(size)));
            var controls = CreateRecursive(grid).ToArray();
            var layout = new Layout(grid, Input, controls);
            layout.Invalidated += _ =>
            {
                Console.WriteLine("inv");
                ResizeRecursive(layout.Size.V, grid, layout.Position.V, s);
            };
            ResizeRecursive(size, grid, layout.Position.V, s);
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
            void ResizeRecursive(Coord screenSize, LayoutGrid grid, Coord p, Vec s, int i = 0)
            {
                if (screenSize == Coord.Zero)
                    return;
                var totalRel = Normalize(grid.Select(x => x.Size.RelativePart).DefaultIfEmpty(new()).Aggregate((a, b) => a + b));
                // If the size of the grid is 0 in either dimension, it's because this is either a row or a column
                // Therefore the size of the other dimension is dictated by the parent
                var relGrid = Normalize(grid.Size.RelativePart);
                var gridSize = grid.Size.AbsolutePart + (screenSize - grid.Size.AbsolutePart) * relGrid * s;
                var availableGridSpace = gridSize - grid.Select(x => x.Size.AbsolutePart).DefaultIfEmpty(new()).Aggregate((a, b) => a + b);

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
                    var rPos = relGrid * child.Position.RelativePart / totalRel;
                    // If the size of the child is 0 in either dimension, yadda yadda
                    var relChild = Normalize(child.Size.RelativePart);
                    var rSize = relGrid * relChild / totalRel;
                    var computedChildPos = (p + child.Position.AbsolutePart + rPos * gridSize).ToCoord();
                    var computedChildSize = (child.Size.AbsolutePart + rSize * (availableGridSpace - child.Size.AbsolutePart)).ToCoord();
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
                    ResizeRecursive(screenSize, child, computedChildPos, rSize, i + 1);
                }

                Vec Normalize(Vec v)
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
