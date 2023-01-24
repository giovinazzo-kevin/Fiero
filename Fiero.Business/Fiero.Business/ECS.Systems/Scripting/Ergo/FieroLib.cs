
using Ergo.Events;
using Ergo.Interpreter.Directives;
using Ergo.Interpreter.Libraries;
using Ergo.Lang.Ast;
using Ergo.Solver.BuiltIns;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business;
public class FieroLib : Library
{
    public override Atom Module => ErgoScriptingSystem.FieroModule;

    protected readonly Dictionary<Atom, HashSet<Signature>> Subscribptions = new();

    public FieroLib()
    {
    }


    public override IEnumerable<SolverBuiltIn> GetExportedBuiltins() => Enumerable.Empty<SolverBuiltIn>()
        ;
    public override IEnumerable<InterpreterDirective> GetExportedDirectives() => Enumerable.Empty<InterpreterDirective>()
        .Append(new SubscribeToEvent())
        ;
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

