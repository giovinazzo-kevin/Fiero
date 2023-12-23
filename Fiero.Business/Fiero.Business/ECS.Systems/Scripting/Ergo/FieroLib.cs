
using Ergo.Events;
using Ergo.Interpreter.Directives;
using Ergo.Interpreter.Libraries;
using Ergo.Lang.Ast;
using Ergo.Runtime.BuiltIns;
using LightInject;

namespace Fiero.Business;

public partial class FieroLib : Library
{
    public override Atom Module => ScriptingSystem.FieroModule;

    protected readonly Dictionary<Atom, HashSet<Signature>> Subscribptions = new();

    public readonly IServiceFactory ServiceFactory;

    private readonly List<BuiltIn> _exportedBuiltIns = new();
    private readonly List<InterpreterDirective> _exportedDirectives = new();

    public FieroLib(IServiceFactory sp)
    {
        ServiceFactory = sp;
        _exportedBuiltIns.Add(ServiceFactory.GetInstance<At>());
        _exportedBuiltIns.Add(ServiceFactory.GetInstance<Shape>());
        _exportedBuiltIns.Add(ServiceFactory.GetInstance<Spawn>());
        _exportedBuiltIns.Add(ServiceFactory.GetInstance<Despawn>());
        _exportedBuiltIns.Add(ServiceFactory.GetInstance<RaiseEvent>());
        _exportedBuiltIns.Add(ServiceFactory.GetInstance<Database>());
        _exportedBuiltIns.Add(ServiceFactory.GetInstance<CastEntity>());
        _exportedBuiltIns.Add(ServiceFactory.GetInstance<ComponentSetValue>());
        _exportedBuiltIns.Add(ServiceFactory.GetInstance<SetRandomSeed>());
        _exportedBuiltIns.Add(ServiceFactory.GetInstance<NextRandom>());
        _exportedBuiltIns.Add(ServiceFactory.GetInstance<MsgBox>());
        _exportedBuiltIns.Add(ServiceFactory.GetInstance<TriggerEffect>());
        _exportedBuiltIns.Add(ServiceFactory.GetInstance<TriggerAnimation>());
        _exportedBuiltIns.Add(ServiceFactory.GetInstance<TriggerAnimationBlocking>());
        _exportedBuiltIns.Add(ServiceFactory.GetInstance<AnimationRepeatCount>());
        _exportedBuiltIns.Add(ServiceFactory.GetInstance<AnimationStop>());
        _exportedBuiltIns.Add(ServiceFactory.GetInstance<TriggerSound>());
        _exportedDirectives.Add(ServiceFactory.GetInstance<SubscribeToEvent>());
        _exportedDirectives.Add(ServiceFactory.GetInstance<SubscribeToEvent>());
    }


    public override IEnumerable<BuiltIn> GetExportedBuiltins() => _exportedBuiltIns;
    public override IEnumerable<InterpreterDirective> GetExportedDirectives() => _exportedDirectives;
    public override void OnErgoEvent(ErgoEvent evt)
    {
        base.OnErgoEvent(evt);
    }

    public void SubscribeScriptToEvent(Atom scriptModule, Atom eventModule, Atom @event)
    {
        if (!Subscribptions.TryGetValue(scriptModule, out var set))
            set = Subscribptions[scriptModule] = new();
        set.Add(new(@event, 1, eventModule, default));
    }

    public Ergo.Lang.Maybe<IEnumerable<Signature>> GetScriptSubscriptions(Script script)
    {
        var set = new HashSet<Signature>();
        var modules = script.ScriptProperties.KnowledgeBase.Scope.VisibleModules;
        foreach (var m in modules)
        {
            if (Subscribptions.TryGetValue(m, out var inner))
                set.UnionWith(inner);
        }
        return set;
    }
}

