using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Exceptions;

namespace Fiero.Business
{
    public class ErgoScriptComponent : EcsComponent
    {
        public ErgoException LastError { get; set; }
        public string CacheKey { get; set; } = string.Empty;
        public string ScriptPath { get; set; }
        public bool ShowTrace { get; set; }
        public bool Cached { get; set; }
        public List<Signature> SubscribedEvents { get; set; } = new();
        public KnowledgeBase KnowledgeBase { get; set; }
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
