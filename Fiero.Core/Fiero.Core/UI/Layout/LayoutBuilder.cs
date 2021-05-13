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
            var controls = CreateRecursive(grid, p, s)
                .ToArray();
            var layout = new Layout(Input, controls);
            layout.Size.ValueChanged += (owner, old) => ResizeRecursive(layout.Size.V, grid, p, s);
            layout.Position.ValueChanged += (owner, old) => ResizeRecursive(layout.Size.V, grid, p, s);
            return layout;

            Func<Coord, Coord, UIControl> GetResolver(Type controlType)
            {
                var resolverType = typeof(IUIControlResolver<>).MakeGenericType(controlType);
                var resolver = ServiceProvider.GetInstance(resolverType);
                var resolveMethod = resolver.GetType().GetMethod(nameof(IUIControlResolver<UIControl>.Resolve));
                return (p, s) => {
                    var control = resolveMethod.Invoke(resolver, new object[] { p, s });
                    return (UIControl)control;
                };
            }

            IEnumerable<UIControl> CreateRecursive(LayoutGrid grid, Vec p, Vec s)
            {
                var v = size.ToVec();
                var cS = s / grid.Size.Clamp(min: 1);
                if (grid.IsCell) {
                    var resolver = GetResolver(grid.ControlType);
                    var (controlPos, controlSize) = ((p * v).ToCoord(), (cS * v).ToCoord());
                    var control = resolver(controlPos, controlSize);
                    if(control != null) {
                        control.Size.V = controlSize;
                        control.Position.V = controlPos;
                        grid.InitializeControl?.Invoke(grid.ControlInstance = control);
                        foreach (var rule in grid.GetStyles(control.GetType())) {
                            rule(control);
                        }
                    }
                }
                var enumerator = grid.GetEnumerator();
                for (int i = 0; i < grid.Cols + 1; i++) {
                    for (int j = 0; j < grid.Rows + 1; j++) {
                        if(!enumerator.MoveNext()) {
                            yield break;
                        }
                        var child = enumerator.Current;
                        var cP = p + cS * new Vec(i, j);
                        foreach (var inner in CreateRecursive(child, cP, cS)) {
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
                var v = size.ToVec();
                var cS = s / grid.Size.Clamp(min: 1);
                var enumerator = grid.GetEnumerator();
                for (int i = 0; i < grid.Cols + 1; i++) {
                    for (int j = 0; j < grid.Rows + 1; j++) {
                        if (!enumerator.MoveNext()) {
                            return;
                        }
                        var child = enumerator.Current;
                        var cP = p + cS * new Vec(i, j);
                        if(child.IsCell && child.ControlInstance != null) {
                            child.ControlInstance.Position.V = layout.Position + (cP * v).ToCoord();
                            child.ControlInstance.Size.V = (cS * v).ToCoord();
                        }
                        ResizeRecursive(size, child, cP, cS);
                    }
                }
            }
        }
    }
}
