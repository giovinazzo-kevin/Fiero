using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using Ergo.Solver;
using Ergo.Solver.BuiltIns;
using Fiero.Core;
using System.Collections.Generic;

namespace Fiero.Business;

[SingletonDependency]
public sealed class ComponentSetValue : GameEntitiesBuiltIn
{
    public ComponentSetValue(GameEntities entities, GameDataStore store)
        : base("", new("component_set_value"), 4, entities, store)
    {
    }

    public override IEnumerable<Evaluation> Apply(SolverContext solver, SolverScope scope, ITerm[] arguments)
    {
        var (component, property, newValue, result) = (arguments[0], arguments[1], arguments[2], arguments[3]);
        if (component.IsAbstract<Dict>().TryGetValue(out var dict) && dict.Signature.Tag.TryGetValue(out var tag))
        {
            var key = tag.Explain();
            if (ProxyableComponentTypes.TryGetValue(key, out var type))
            {
                // This is a copy of the original for now
                EcsComponent actualComponent = (EcsComponent)TermMarshall.FromTerm(component, type, mode: TermMarshalling.Named);
                var args = new object[] { actualComponent.Id, default(EcsComponent) };
                if ((bool)TryGetComponent.MakeGenericMethod(type).Invoke(Entities, args))
                {
                    // Now it's the actual one so we can change its properties
                    actualComponent = (EcsComponent)args[1];
                    if (property.Matches(out string propName) && ProxyableComponentProperties[key].TryGetValue(propName, out var prop))
                    {
                        var newValueObject = TermMarshall.FromTerm(newValue, prop.PropertyType, mode: TermMarshalling.Named);
                        prop.SetValue(actualComponent, newValueObject);
                        var match = TermMarshall.ToTerm(actualComponent, type, mode: TermMarshalling.Named);
                        if (result.Unify(match).TryGetValue(out var subs))
                        {
                            yield return True(subs);
                            yield break;
                        }
                        yield return False();
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
