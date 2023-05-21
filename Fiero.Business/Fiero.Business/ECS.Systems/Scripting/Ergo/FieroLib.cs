
using Ergo.Events;
using Ergo.Interpreter.Directives;
using Ergo.Interpreter.Libraries;
using Ergo.Lang.Ast;
using Ergo.Solver.BuiltIns;
using LightInject;
using System.Collections.Generic;

namespace Fiero.Business;
public class FieroLib : Library
{
    public override Atom Module => ErgoScriptingSystem.FieroModule;

    protected readonly Dictionary<Atom, HashSet<Signature>> Subscribptions = new();

    public readonly IServiceFactory ServiceFactory;

    private readonly List<SolverBuiltIn> _exportedBuiltIns = new();
    private readonly List<InterpreterDirective> _exportedDirectives = new();

    public FieroLib(IServiceFactory sp)
    {
        ServiceFactory = sp;
        _exportedBuiltIns.Add(ServiceFactory.GetInstance<Spawn>());
        _exportedBuiltIns.Add(ServiceFactory.GetInstance<SetRngSeed>());
        _exportedDirectives.Add(ServiceFactory.GetInstance<SubscribeToEvent>());
    }


    public override IEnumerable<SolverBuiltIn> GetExportedBuiltins() => _exportedBuiltIns;
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
        if (Subscribptions.TryGetValue(script.ScriptProperties.Scope.InterpreterScope.Entry, out var set))
            return set;
        return default;
    }
}

