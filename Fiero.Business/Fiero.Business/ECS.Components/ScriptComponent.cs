using Ergo.Lang.Ast;
using Ergo.Solver;
using Ergo.Solver.DataBindings;
using Fiero.Core;
using System.Collections.Generic;

namespace Fiero.Business
{
    public class ErgoScriptComponent : EcsComponent
    {
        public string ScriptPath { get; set; }
        public SolverScope Scope { get; set; }
        public ErgoSolver Solver { get; set; }
        public List<Signature> SubscribedEvents { get; set; }
        public DataSink<Script.Stdout> Stdout { get; set; }
    }
}
