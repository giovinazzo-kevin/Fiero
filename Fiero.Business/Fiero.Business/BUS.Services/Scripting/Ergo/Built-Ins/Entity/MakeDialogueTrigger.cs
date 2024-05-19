using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using Ergo.Runtime;
using Ergo.Runtime.BuiltIns;
using LightInject;
using System.Reflection;

namespace Fiero.Business;

[SingletonDependency]
public sealed class MakeDialogueTrigger : BuiltIn
{
    [Term(Functor = "trigger_def", Marshalling = TermMarshalling.Named)]
    internal readonly record struct DialogueTriggerDef()
    {
        public readonly bool Repeatable { get; init; } = false;
        public readonly string[] Choices { get; init; } = [];
    };

    private static readonly Dictionary<Atom, Type> ValidTriggerTypes = Assembly.GetExecutingAssembly()
        .GetTypes()
        .Where(t => t.IsAssignableTo(typeof(IDialogueTrigger)) && !t.IsAbstract)
        .ToDictionary(t => new Atom(t.Name.Replace("Dialogue", string.Empty).Replace("Trigger", string.Empty).ToErgoCase()));

    private IServiceFactory _services;
    public MakeDialogueTrigger(IServiceFactory services)
        : base("", new("dialogue_trigger"), 2, FieroLib.Modules.Entity)
    {
        _services = services;
    }

    public override ErgoVM.Op Compile()
    {
        var systems = _services.GetInstance<MetaSystem>();
        var resources = _services.GetInstance<GameResources>();
        return vm =>
        {
            var args = vm.Args;
            if (!args[0].IsAbstract<Dict>().TryGetValue(out var dict))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, WellKnown.Types.Dictionary, args[0]);
                return;
            }
            if (!dict.Functor.TryGetA(out var functor))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, WellKnown.Types.Functor, args[0]);
                return;
            }
            if (!ValidTriggerTypes.TryGetValue(functor, out var triggerType))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, nameof(DialogueTrigger) + "Type", args[0]);
                return;
            }
            if (!args[0].Match(out DialogueTriggerDef stub, matchFunctor: false))
            {
                vm.Throw(ErgoVM.ErrorType.ExpectedTermOfTypeAt, nameof(DialogueTriggerDef), args[0]);
                return;
            }
            // Signature: (MetaSystem sys, bool repeatable, params string[] nodeChoices)
            var inst = Activator.CreateInstance(triggerType, [systems, stub.Repeatable, stub.Choices]);
            vm.SetArg(0, new Atom(new TermMarshall.Unmarshalled(inst))); // prevent marshalling, ensuring this value crosses the C#/Ergo barrier unscathed
            ErgoVM.Goals.Unify2(vm);
        };
    }
}
