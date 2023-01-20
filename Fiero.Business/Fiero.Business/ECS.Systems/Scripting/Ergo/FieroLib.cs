
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

    protected readonly Dictionary<Atom, List> Subscribptions = new();

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

    public void SubscribeScriptToEvents(Atom scriptModule, List events)
    {
        Subscribptions[scriptModule] = events;
    }

    public Ergo.Lang.Maybe<List> GetScriptSubscriptions(Script script)
    {
        if (Subscribptions.TryGetValue(script.ScriptProperties.Scope.InterpreterScope.Entry, out var list))
            return list;
        return default;
    }
}

