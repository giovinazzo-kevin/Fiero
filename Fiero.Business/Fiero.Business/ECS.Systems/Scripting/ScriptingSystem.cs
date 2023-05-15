using Ergo.Facade;
using Ergo.Interpreter;
using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Exceptions;
using Ergo.Lang.Extensions;
using Ergo.Solver;
using Fiero.Core;
using LightInject;
using System;
using System.Collections.Generic;
using System.Linq;
using Unconcern.Common;

namespace Fiero.Business
{
    public partial class ErgoScriptingSystem : EcsSystem
    {
        public static readonly Atom FieroModule = new("fiero");
        protected static readonly Dictionary<Signature, Func<ScriptEffect, GameSystems, Subscription>> CachedRoutes =
            GetScriptRoutes();

        public readonly ErgoFacade Facade;
        public readonly ErgoInterpreter Interpreter;
        public readonly FieroLib FieroLib;
        public InterpreterScope Scope;

        public readonly SystemRequest<ErgoScriptingSystem, ScriptLoadedEvent, EventResult> ScriptLoaded;
        public readonly SystemEvent<ErgoScriptingSystem, InputAvailableEvent> InputAvailable;

        public ErgoScriptingSystem(EventBus bus, IServiceFactory sp) : base(bus)
        {
            FieroLib = new(sp);
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
            InputAvailable = new(this, nameof(InputAvailable));
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
            if (script.IsInvalid())
                return false;
            var localScope = Scope
                .WithExceptionHandler(new(
                    @catch: ex =>
                    {
                        script.ScriptProperties.LastError = ex;
                        // TODO: Use script's stderr!!
                        Console.WriteLine(ex);
                    },
                    @finally: () =>
                    {

                    }));
            if (Interpreter.Load(ref localScope, new Atom(script.ScriptProperties.ScriptPath)).TryGetValue(out var module))
            {
                var solver = Facade.BuildSolver(
                    localScope.BuildKnowledgeBase(),
                    SolverFlags.Default & ~SolverFlags.ThrowOnPredicateNotFound
                );
                var solverScope = solver.CreateScope(localScope);
                script.ScriptProperties.Solver = solver;
                script.ScriptProperties.Scope = solverScope;
                // Scripts subscribe to events via the subscribe/2 directive
                if (!FieroLib.GetScriptSubscriptions(script).TryGetValue(out var subbedEvents))
                    subbedEvents = Enumerable.Empty<Signature>();
                // Effects can then read this list and bind the subbed events
                script.ScriptProperties.SubscribedEvents.AddRange(subbedEvents);
                solver.Initialize(localScope);
                ScriptLoaded.Handle(new(script));
                return true;
            }
            return false;
        }

        // TODO: Figure out script lifetime and call this method!!
        public bool UnloadScript(Script script)
        {
            if (script.IsInvalid())
                return false;
            script.ScriptProperties.Solver?.Dispose();
            script.ScriptProperties.Solver = default;
            return true;
        }

        /// <summary>
        /// Maps every SystemRequest and SystemEvent defined in all systems to a wrapper that
        /// calls an Ergo script automatically and parses its result as an EventResult for Fiero,
        /// returning a subscription that will be disposed when this effect ends.
        /// </summary>
        /// <returns>All routes indexed by signature.</returns>
        public static Dictionary<Signature, Func<ScriptEffect, GameSystems, Subscription>> GetScriptRoutes()
        {
            if (CachedRoutes != null)
                return CachedRoutes;

            var finalDict = new Dictionary<Signature, Func<ScriptEffect, GameSystems, Subscription>>();
            foreach (var (sys, field, isReq) in MetaSystem.GetSystemEventFields())
            {
                var sysName = new Atom(sys.Name.Replace("System", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .ToErgoCase());
                if (isReq)
                {
                    var reqName = new Atom(field.Name.Replace("Request", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .ToErgoCase());
                    var reqType = field.FieldType.GetGenericArguments()[1];
                    finalDict.Add(new(reqName, 1, sysName, default), (self, systems) =>
                    {
                        return ((ISystemRequest)field.GetValue(sys.GetValue(systems)))
                            .SubscribeResponse(evt => Respond(self, evt, reqType, reqName, sysName));
                    });
                }
                else
                {
                    var evtName = new Atom(field.Name.Replace("Event", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .ToErgoCase());
                    var evtType = field.FieldType.GetGenericArguments()[1];
                    finalDict.Add(new(evtName, 1, sysName, default), (self, systems) =>
                    {
                        return ((ISystemEvent)field.GetValue(sys.GetValue(systems)))
                            .SubscribeHandler(evt => Respond(self, evt, evtType, evtName, sysName));
                    });
                }
            }
            return finalDict;


            static EventResult Respond(ScriptEffect self, object evt, Type type, Atom evtName, Atom sysName)
            {
                var term = TermMarshall.ToTerm(evt, type, mode: TermMarshalling.Named);
                // Qualify term with module so that the declaration needs to match, e.g. action:actor_turn_started/1
                var query = new Query(((ITerm)new Complex(evtName, term)).Qualified(sysName));
                try
                {
                    // TODO: Figure out a way for scripts to return complex EventResults?
                    foreach (var _ in self.Script.Solve(query))
                    {
                        if (self.Script.ScriptProperties.LastError != null)
                            return false;
                    }
                    return true;
                }
                catch (ErgoException ex)
                {
                    // TODO: Log to the in-game console
                    Console.WriteLine(ex);
                    return false;
                }
            }
        }
    }
}
