using Ergo.Lang.Ast;
using Ergo.Solver;
using Fiero.Core;
using System.Collections.Generic;

namespace Fiero.Business
{
    public class Script : Entity
    {
        [RequiredComponent]
        public ErgoScriptComponent ScriptProperties { get; private set; }

        public IEnumerable<Solution> Solve(Query q) => ScriptProperties.Solver.Solve(q, ScriptProperties.Scope);
    }
}
