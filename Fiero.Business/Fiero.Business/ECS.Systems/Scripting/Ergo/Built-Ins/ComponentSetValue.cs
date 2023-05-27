using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Exceptions;
using Ergo.Lang.Extensions;
using Ergo.Solver;
using Ergo.Solver.BuiltIns;
using Fiero.Core;
using System;
using System.Collections.Generic;

namespace Fiero.Business;

[SingletonDependency]
public sealed class ComponentSetValue : GameEntitiesBuiltIn
{
    public ComponentSetValue(GameEntities entities, GameDataStore store)
        : base("", new("component_set_value"), 3, entities, store)
    {
    }

    public override IEnumerable<Evaluation> Apply(SolverContext solver, SolverScope scope, ITerm[] arguments)
    {
        var (component, property, newValue) = (arguments[0], arguments[1], arguments[2]);
        if (component.IsAbstract<Dict>().TryGetValue(out var dict) && dict.Signature.Tag.TryGetValue(out var tag))
        {
            var key = tag.Explain();
            if (ProxyableComponentTypes.TryGetValue(key, out var type))
            {
                // This is a copy of the original for now
                if (!scope.InterpreterScope.ExceptionHandler.TryGet(() =>
                    (EcsComponent)TermMarshall.FromTerm(component, type, mode: TermMarshalling.Named))
                    .TryGetValue(out var actualComponent))
                {
                    yield return ThrowFalse(scope, Ergo.Lang.Exceptions.SolverError.ExpectedTermOfTypeAt, FieroLib.Types.Component, property.Explain());
                    yield break;
                }
                var args = new object[] { actualComponent.Id, default(EcsComponent) };
                if ((bool)TryGetComponent.MakeGenericMethod(type).Invoke(Entities, args))
                {
                    // Now it's the actual one so we can change its properties
                    actualComponent = (EcsComponent)args[1];
                    if (property.Matches(out string propName) && ProxyableComponentProperties[key].TryGetValue(propName, out var prop))
                    {
                        var ergoType = prop.PropertyType.Name.ToErgoCase();
                        if (!scope.InterpreterScope.ExceptionHandler.Try(() =>
                        {
                            var newValueObject = TermMarshall.FromTerm(newValue, prop.PropertyType, mode: TermMarshalling.Named);
                            try
                            {
                                prop.SetValue(actualComponent, newValueObject);
                            }
                            catch (ArgumentException)
                            {
                                throw new InternalErgoException($"Can not convert atom {newValue.Explain()} to type {ergoType}");
                            }
                        }))
                        {
                            yield return ThrowFalse(scope, SolverError.ExpectedTermOfTypeAt, ergoType, property.Explain());
                            yield break;
                        }
                        yield return True();
                        yield break;
                    }
                    yield return ThrowFalse(scope, Ergo.Lang.Exceptions.SolverError.ExpectedTermOfTypeAt, FieroLib.Types.ComponentProperty, property.Explain());
                    yield break;
                }
            }
            yield return ThrowFalse(scope, Ergo.Lang.Exceptions.SolverError.ExpectedTermOfTypeAt, FieroLib.Types.ComponentType, tag.Explain());
            yield break;
        }
        yield return ThrowFalse(scope, Ergo.Lang.Exceptions.SolverError.ExpectedTermOfTypeAt, FieroLib.Types.Component, component.Explain());
        yield break;
    }
}
