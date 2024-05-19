using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using Ergo.Runtime;
using Ergo.Runtime.BuiltIns;
using LightInject;
using System.Reflection;

namespace Fiero.Business;

public abstract class TriggerAnimationBase(IServiceFactory services, string name) : BuiltIn("", new(name), 3, FieroLib.Modules.Animation)
{
    protected readonly IServiceFactory Services = services;
    private readonly Dictionary<string, MethodInfo> Methods = typeof(Animation)
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .Where(m => m.ReturnType == typeof(Animation))
            .ToDictionary(m => m.Name.ToErgoCase());

    protected bool IsBlocking { get; set; } = false;

    public override ErgoVM.Op Compile()
    {
        return vm =>
        {
            var args = vm.Args;
            var at = default(Either<Location, PhysicalEntity>);
            if (args[0].IsAbstract<EntityAsTerm>().TryGetValue(out var entityAsTerm)
                && entityAsTerm.GetProxy().TryGetValue(out var proxy)
                && proxy is PhysicalEntity pe)
            {
                at = pe;
            }
            else if (args[0].Match(out Location location))
            {
                at = location;
            }
            else
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, nameof(Location), args[0]);
                return;
            }
            if (args[1] is not List list)
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, WellKnown.Types.List, args[2]);
                return;
            }
            var animList = new List<Animation>();
            foreach (var anim in list.Contents)
            {
                if (anim is not Dict dict)
                {
                    vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, WellKnown.Types.Dictionary, anim);
                    return;
                }
                if (!dict.Functor.TryGetA(out var functor))
                {
                    vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, WellKnown.Types.Functor, anim);
                    return;
                }
                if (!Methods.TryGetValue(functor.Explain(), out var method))
                {
                    vm.Fail();
                    return;
                }
                var oldParams = method.GetParameters();
                var newParams = new object[oldParams.Length];
                for (int i = 0; i < oldParams.Length; i++)
                {
                    var p = oldParams[i];
                    if (dict.Dictionary.TryGetValue(new Atom(p.Name.ToErgoCase()), out var value)
                    && TermMarshall.FromTerm(value, p.ParameterType) is { } val)
                    {
                        newParams[i] = val;
                    }
                    else if (p.HasDefaultValue)
                    {
                        newParams[i] = p.DefaultValue;
                    }
                    else
                    {
                        vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, p.ParameterType.Name, p);
                        return;
                    }
                }
                animList.Add((Animation)method.Invoke(null, newParams));
            }
            var renderSystem = Services.GetInstance<RenderSystem>();
            var lastId = renderSystem.AnimateViewport(IsBlocking, at, [.. animList]);
            var idList = Enumerable.Range(lastId - animList.Count, animList.Count);
            vm.SetArg(0, args[2]);
            vm.SetArg(1, new List(idList.Select(x => new Atom(x + 1)).Cast<ITerm>()));
            ErgoVM.Goals.Unify2(vm);
        };
    }
}
