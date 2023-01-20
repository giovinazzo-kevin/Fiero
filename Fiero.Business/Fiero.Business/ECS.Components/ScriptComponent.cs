using Ergo.Lang.Ast;
using Ergo.Solver;
using Fiero.Core;

namespace Fiero.Business
{
    public class ErgoScriptComponent : EcsComponent
    {
        public string ScriptPath { get; set; }
        public SolverScope Scope { get; set; }
        public ErgoSolver Solver { get; set; }
        public List SubscribedEvents { get; set; }
    }
}
