using Ergo.Lang.Ast;
using Ergo.Lang.Exceptions;
using Ergo.Solver;

namespace Fiero.Business
{
    public class ErgoScriptComponent : EcsComponent
    {
        public ErgoException LastError { get; set; }
        public string ScriptPath { get; set; }
        public bool ShowTrace { get; set; }
        public SolverScope Scope { get; set; }
        public ErgoSolver Solver { get; set; }
        public List<Signature> SubscribedEvents { get; set; } = new();
        /// <summary>
        /// Output pipe for the Solver's Out stream. You read here what is written there.
        /// </summary>
        public TextReader Out { get; set; }
        /// <summary>
        /// Input pipe for the Solver's In stream. You write here what the solver reads there.
        /// </summary>
        public TextWriter In { get; set; }
    }
}
