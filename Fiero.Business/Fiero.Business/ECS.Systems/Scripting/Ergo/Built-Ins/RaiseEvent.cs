﻿using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using Ergo.Runtime;
using Ergo.Runtime.BuiltIns;
using LightInject;
using System.Reflection;

namespace Fiero.Business;

[SingletonDependency]
public sealed class RaiseEvent : BuiltIn
{
    private IServiceFactory _services;
    public RaiseEvent(IServiceFactory services)
        : base("", new("raise"), 3, ScriptingSystem.EventModule)
    {
        _services = services;
    }

    public override ErgoVM.Op Compile()
    {
        var meta = _services.GetInstance<MetaSystem>();
        var gameSystems = _services.GetInstance<GameSystems>();
        return vm =>
        {
            var arguments = vm.Args;
            if (!arguments[0].Matches(out string sysName))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, WellKnown.Types.String, arguments[0]);
                return;
            }
            if (!arguments[1].Matches(out string eventname))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, WellKnown.Types.String, arguments[1]);
                return;
            }
            if (!arguments[2].IsAbstract<Dict>().TryGetValue(out var dict))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, WellKnown.Types.Dictionary, arguments[2]);
                return;
            }
            var any = false; var anySystem = false;
            foreach (var field in meta.GetSystemEventFields())
            {
                if (field.System.GetType().Name.ToErgoCase().Replace("System", string.Empty, StringComparison.OrdinalIgnoreCase) != sysName)
                    continue;
                anySystem = true;
                if (field.Field.Name.ToErgoCase()
                    .Replace("Event", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Replace("Request", string.Empty, StringComparison.OrdinalIgnoreCase)
                    != eventname)
                    continue;
                var b = field.Field.FieldType.BaseType;
                if (!b.IsGenericType || !b.GetGenericTypeDefinition().IsAssignableFrom(typeof(SystemEvent<,>)))
                {
                    vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, "SystemEvent", field);
                    return;
                }
                var tArgs = field.Field.FieldType.BaseType.GetGenericArguments()[1];
                var obj = field.Field.GetValue(field.System);
                var arg = TermMarshall.FromTerm(arguments[2], tArgs, TermMarshalling.Named);
                var ret = field.Field.FieldType.GetMethod("Raise", BindingFlags.Public | BindingFlags.Instance)
                    .Invoke(obj, new[] { arg, default(CancellationToken) });
                // TODO: call Handle if it's a request and return false if Handle does so
                any = true;
            }
            if (any)
            {
                return;
            }
            // If this event doesn't pertain to any builtin system, then the event will be sent as a script event.
            // It can be sent from any module, even the modules of other scripts, with the system name acting as a conventional tag.
            if (!anySystem)
            {
                _ = gameSystems.Scripting.ScriptEventRaised.Raise(new(sysName, eventname, dict));
                return;
            }
        };
    }
}