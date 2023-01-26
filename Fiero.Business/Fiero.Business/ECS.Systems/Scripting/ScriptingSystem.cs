using Ergo.Facade;
using Ergo.Interpreter;
using Ergo.Lang.Ast;
using Ergo.Solver;
using Ergo.Solver.DataBindings;
using Fiero.Core;
using System;
using System.Linq;
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
            Scope = Interpreter.CreateScope(stdlib => stdlib)
                .WithSearchDirectory(@".\Resources\Scripts\")
                .WithRuntime(true)
                ;
            var fiero = Interpreter.Load(ref Scope, FieroModule)
                .GetOrThrow(new InvalidOperationException())
                .WithLinkedLibrary(FieroLib);
            Scope = Scope
                .WithModule(fiero)
                .WithModule(Scope.Modules[WellKnown.Modules.Stdlib]
                    .WithImport(FieroModule));

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
                // Scripts subscribe to events via the subscribe/1 directive
                if (!FieroLib.GetScriptSubscriptions(script).TryGetValue(out var subbedEvents))
                    subbedEvents = Enumerable.Empty<Signature>();
                // Effects can then read this list and bind the subbed events
                script.ScriptProperties.SubscribedEvents = new(subbedEvents);
                // All write_* predicates are routed to the script's stdout via the io:portray/1 hook (except write_raw/1 which skips the hook)
                // TODO: watch https://github.com/G3Kappa/Ergo/issues/60 and then implement the necessary changes
                script.ScriptProperties.Stdout = new DataSink<Script.Stdout>(new Atom("stdout"));
                solver.BindDataSink(script.ScriptProperties.Stdout);
                // All write_* predicates are routed to the script's stdout via the io:portray/1 hook (except write_raw/1 which skips the hook)
                // TODO: watch https://github.com/G3Kappa/Ergo/issues/60 and then implement the necessary changes
                script.ScriptProperties.Stdout = new DataSink<Script.Stdout>(new Atom("stdout"));
                solver.BindDataSink(script.ScriptProperties.Stdout);
                solver.Initialize(localScope);
                ScriptLoaded.Handle(new(script));
                return true;
            }
            return false;
        }
    }
}
