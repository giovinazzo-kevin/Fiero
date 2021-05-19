using LightInject;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
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
            var p = new Vec(0f, 0f);
            var s = new Vec(1f, 1f);
            var grid = build(new());
            var controls = CreateRecursive(grid).ToArray();
            var layout = new Layout(grid, Input, controls);
            layout.Size.ValueChanged += (owner, old) => ResizeRecursive(layout.Size.V, grid, p, s);
            layout.Position.ValueChanged += (owner, old) => ResizeRecursive(layout.Size.V, grid, p, s);
            ResizeRecursive(size, grid, p, s);
            return layout;

            Func<UIControl> GetResolver(Type controlType)
            {
                var resolverType = typeof(IUIControlResolver<>).MakeGenericType(controlType);
                var resolver = ServiceProvider.GetInstance(resolverType);
                var resolveMethod = resolver.GetType().GetMethod(nameof(IUIControlResolver<UIControl>.Resolve));
                return () => {
                    var control = resolveMethod.Invoke(resolver, new object[] { grid });
                    return (UIControl)control;
                };
            }

            IEnumerable<UIControl> CreateRecursive(LayoutGrid grid)
            {
                if (grid.IsCell) {
                    var resolver = GetResolver(grid.ControlType);
                    var control = resolver();
                    if(control != null) {
                        grid.InitializeControl?.Invoke(grid.ControlInstance = control);
                    }
                }

                var enumerator = grid.GetEnumerator();
                for (int i = 0; i < grid.Cols + 1; i++) {
                    for (int j = 0; j < grid.Rows + 1; j++) {
                        if(!enumerator.MoveNext()) {
                            yield break;
                        }
                        var child = enumerator.Current;
                        foreach (var inner in CreateRecursive(child)) {
                            yield return inner;
                        }
                        if(child.IsCell && child.ControlInstance != null) {
                            yield return child.ControlInstance;
                        }
                    }
                }
            }

            void ResizeRecursive(Coord size, LayoutGrid grid, Vec p, Vec s)
            {
                var sz = size.ToVec();
                var gk = grid.Subdivisions.Clamp(min: 1);
                foreach (var child in grid) {
                    var cPos = p + child.Position / gk;
                    var cSize = child.Size * s / gk;
                    if(child.IsCell && child.ControlInstance != null) {
                        child.ControlInstance.Position.V = layout.Position + (cPos * sz).ToCoord();
                        child.ControlInstance.Size.V = (cSize * sz).ToCoord();
                        foreach (var rule in grid.GetStyles(child.ControlType)) {
                            rule(child.ControlInstance);
                        }
                    }
                    ResizeRecursive(size, child, cPos, cSize);
                }
            }
        }
    }
}
