using Ergo.Facade;
using Ergo.Interpreter;
using Ergo.Lang.Ast;
using Ergo.Solver;
using Fiero.Core;
using Unconcern.Common;

namespace Fiero.Business
{
    public partial class ErgoScriptingSystem : EcsSystem
    {
        public static readonly Atom FieroModule = new("fiero");


        protected readonly ErgoFacade Facade;
        protected readonly ErgoInterpreter Interpreter;
        protected readonly FieroLib FieroLib;
        protected InterpreterScope Scope;

        public readonly SystemRequest<ErgoScriptingSystem, ScriptLoadedEvent, EventResult> ScriptLoaded;

        public ErgoScriptingSystem(EventBus bus) : base(bus)
        {
            FieroLib = new();
            Facade = GetErgoFacade();
            Interpreter = Facade.BuildInterpreter(
                InterpreterFlags.Default
            );
            Scope = Interpreter.CreateScope(stdlib => stdlib
                    .WithModule(new Module(FieroModule, runtime: true)
                        .WithLinkedLibrary(FieroLib))
                    .WithModule(stdlib.EntryModule
                        .WithImport(FieroModule)))
                .WithSearchDirectory(@".\Resources\Scripts\")
                .WithRuntime(true)
                ;

            ScriptLoaded = new(this, nameof(ScriptLoaded));
        }

        /// <summary>
        /// Configures the Ergo environment before the interpreter is created.
        /// </summary>
        protected virtual ErgoFacade GetErgoFacade()
        {
            return ErgoFacade.Standard
                .AddLibrary(() => FieroLib)
                ;
        }

        public bool LoadScript(Script script)
        {
            var localScope = Scope;
            if (Interpreter.Load(ref localScope, new Atom(script.ScriptProperties.ScriptPath)).TryGetValue(out var module))
            {
                var solver = Facade.BuildSolver(
                    localScope.BuildKnowledgeBase(),
                    SolverFlags.Default & ~SolverFlags.ThrowOnPredicateNotFound
                );
                var solverScope = solver.CreateScope(localScope);
                script.ScriptProperties.Solver = solver;
                script.ScriptProperties.Scope = solverScope;
                // TODO: Define directive to declare events that the script listens for
                // Parse it here and forward those declarations to the GameplayScene
                // which will connect each event to the matching SystemRequest in the target system
                // by notifying the script whenever the request is handled
                if (!FieroLib.GetScriptSubscriptions(script).TryGetValue(out var subbedEvents))
                    subbedEvents = List.Empty;
                script.ScriptProperties.SubscribedEvents = subbedEvents;
                ScriptLoaded.Handle(new(script));
                return true;
            }
            return false;
        }

    }
}
