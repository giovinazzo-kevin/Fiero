using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Exceptions;
using Ergo.Lang.Extensions;
using Ergo.Runtime;

namespace Fiero.Business;

[SingletonDependency]
public sealed class ComponentSetValue(GameEntities entities, GameDataStore store) : GameEntitiesBuiltIn("", new("component_set_value"), 3, entities, store)
{
    public override ErgoVM.Op Compile()
    {
        return vm =>
        {
            var arguments = vm.Args;
            var (component, property, newValue) = (arguments[0], arguments[1], arguments[2]);
            if (component is Dict dict && dict.Signature.Tag.TryGetValue(out var tag))
            {
                var key = tag.Explain();
                if (ProxyableComponentTypes.TryGetValue(key, out var type))
                {
                    // This is a copy of the original for now
                    if (!vm.KB.Scope.ExceptionHandler.TryGet(() =>
                    {
                        try
                        {
                            return (EcsComponent)TermMarshall.FromTerm(component, type);
                        }
                        catch (Exception x)
                        {
                            throw new InternalErgoException($"Can not convert term {component.Explain()} to type {type}");
                        }
                    })
                        .TryGetValue(out var actualComponent))
                    {
                        vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, FieroLib.Types.Component, property.Explain());
                        return;
                    }
                    var args = new object[] { actualComponent.Id, default(EcsComponent) };
                    if ((bool)TryGetComponent.MakeGenericMethod(type).Invoke(Entities, args))
                    {
                        // Now it's the actual one so we can change its properties
                        actualComponent = (EcsComponent)args[1];
                        if (property.Match(out string propName) && ProxyableComponentProperties[key].TryGetValue(propName, out var prop))
                        {
                            var ergoType = prop.PropertyType.Name.ToErgoCase();
                            if (!vm.KB.Scope.ExceptionHandler.Try(() =>
                                    {
                                        var newValueObject = TermMarshall.FromTerm(newValue, prop.PropertyType);
                                        try
                                        {
                                            prop.SetValue(actualComponent, newValueObject);
                                        }
                                        catch (Exception)
                                        {
                                            throw new InternalErgoException($"Can not convert atom {newValue.Explain()} to type {ergoType}");
                                        }
                                    }))
                            {
                                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, ergoType, property.Explain());
                                return;
                            }
                        }
                        else
                        {
                            vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, FieroLib.Types.ComponentProperty, property.Explain());
                            return;
                        }
                    }
                }
                else
                {
                    vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, FieroLib.Types.ComponentType, tag.Explain());
                    return;
                }
            }
            else
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, FieroLib.Types.Component, component.Explain());
                return;
            }
        };
    }
}
