using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Compiler;
using Ergo.Lang.Extensions;
using Ergo.Runtime;
using Ergo.Runtime.BuiltIns;
using System.Reflection;

namespace Fiero.Core.Ergo.Libraries.Core.Event;

[SingletonDependency]
public sealed class Raise(MetaSystem meta) : BuiltIn("", new("raise"), 3, CoreErgoModules.Event)
{
    public readonly record struct EventDispatchInfo(Type ArgType, object Target, MethodInfo Raise)
    {
        public object Invoke(object Arg)
        {
            if (Raise.IsStatic)
                return Raise.Invoke(null, [Target, Arg, default(CancellationToken)]);
            else
                return Raise.Invoke(Target, [Arg, default(CancellationToken)]);
        }
    }

    private Maybe<EventDispatchInfo> GetDispatchInfo(string sysName, string eventName)
    {
        foreach (var field in meta.GetSystemEventFields())
        {
            var fieldSysName = field.System.GetType().Name.Replace("System", string.Empty, StringComparison.OrdinalIgnoreCase).ToErgoCase();
            if (fieldSysName != sysName)
                continue;

            var fieldEventName = field.Field.Name
                .Replace("Event", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("Request", string.Empty, StringComparison.OrdinalIgnoreCase)
                .ToErgoCase();
            if (fieldEventName != eventName)
                continue;
            var b = field.Field.FieldType.BaseType;
            if (!b.IsGenericType || !b.GetGenericTypeDefinition().IsAssignableFrom(typeof(SystemEvent<,>)))
            {
                continue; // TODO: Log?
            }
            var tArgs = field.Field.FieldType.BaseType.GetGenericArguments()[1];
            var obj = field.Field.GetValue(field.System);
            if (field.Field.FieldType.GetGenericTypeDefinition().IsAssignableFrom(typeof(SystemRequest<,,>)))
            {
                var tSys = field.Field.FieldType.GetGenericArguments()[0];
                var raise = typeof(EventsExtensions).GetMethod("HandleAsync", BindingFlags.Public | BindingFlags.Static)
                    .MakeGenericMethod([tSys, tArgs]);
                return new EventDispatchInfo(tArgs, obj, raise);
            }
            else
            {
                var raise = field.Field.FieldType.GetMethod("Raise", BindingFlags.Public | BindingFlags.Instance);
                return new EventDispatchInfo(tArgs, obj, raise);
            }
        }
        return default;
    }

    public override ExecutionNode Optimize(BuiltInNode node)
    {
        if (!node.Args[0].IsGround || !node.Args[0].Matches(out string sysName))
            return node;
        if (!node.Args[1].IsGround || !node.Args[1].Matches(out string eventName))
            return node;
        // If the event can be resolved at compile-time, then let's do so.
        if (GetDispatchInfo(sysName, eventName).TryGetValue(out var dispatch))
        {
            return new VirtualNode(vm =>
            {
                var arg = node.Args[2].Substitute(vm.Environment);
                var obj = TermMarshall.FromTerm(arg, dispatch.ArgType, TermMarshalling.Named);
                var ret = dispatch.Invoke(obj);
            });
        }
        else
        {
            return new VirtualNode(vm =>
            {
                var arg = node.Args[2].Substitute(vm.Environment);
                if (!arg.IsAbstract<Dict>().TryGetValue(out var dict))
                {
                    vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, WellKnown.Types.Dictionary, arg);
                    return;
                }
                _ = meta.ScriptEventRaised.Raise(new(sysName, eventName, dict));
            });
        }
    }

    public override ErgoVM.Op Compile()
    {
        // This will only get called when sysName or eventName are variables at compile-time.
        // Otherwise, Optimize() replaces this node with a VirtualNode containing the correct Op.
        return vm =>
        {
            var arguments = vm.Args;
            if (!arguments[0].Matches(out string sysName))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, WellKnown.Types.String, arguments[0]);
                return;
            }
            if (!arguments[1].Matches(out string eventName))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, WellKnown.Types.String, arguments[1]);
                return;
            }
            if (!arguments[2].IsAbstract<Dict>().TryGetValue(out var dict))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, WellKnown.Types.Dictionary, arguments[2]);
                return;
            }
            if (GetDispatchInfo(sysName, eventName).TryGetValue(out var dispatch))
            {
                var arg = TermMarshall.FromTerm(arguments[2], dispatch.ArgType, TermMarshalling.Named);
                var ret = dispatch.Invoke(arg);
            }
            else
            {
                // If this event doesn't pertain to any builtin system, then the event will be sent as a script event.
                // It can be sent from any module, even the modules of other scripts, with the system name acting as a conventional tag.
                _ = meta.ScriptEventRaised.Raise(new(sysName, eventName, dict));
            }
        };
    }
}