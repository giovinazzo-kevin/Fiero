using Ergo.Facade;
using Ergo.Interpreter;
using Ergo.Interpreter.Libraries;
using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Exceptions;
using Ergo.Lang.Exceptions.Handler;
using Ergo.Lang.Extensions;
using Ergo.Shell;
using Ergo.Solver;
using LightInject;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.IO.Pipelines;
using System.Text;
using Unconcern.Common;

namespace Fiero.Business
{
    public partial class ErgoScriptingSystem : EcsSystem
    {
        public static readonly Atom FieroModule = new("fiero");
        public static readonly Atom AnimationModule = new("anim");
        public static readonly Atom SoundModule = new("sound");
        public static readonly Atom EffectModule = new("fx");
        protected static readonly Dictionary<Signature, Func<ScriptEffect, GameSystems, Subscription>> CachedRoutes =
            GetScriptRoutes();

        public readonly ErgoFacade Facade;
        public readonly ErgoShell Shell;
        public readonly ErgoInterpreter Interpreter;
        public readonly FieroLib FieroLib;
        public readonly InterpreterScope StdlibScope;

        public readonly Pipe In = new(), Out = new();
        public readonly TextWriter InWriter, OutWriter;
        public TextReader InReader { get; private set; }
        public readonly TextReader OutReader;
        public readonly IAsyncInputReader AsyncInputReader;

        private event Action _unload;

        public readonly Encoding Encoding = Encoding.GetEncoding(437);
        public readonly SystemRequest<ErgoScriptingSystem, ScriptLoadedEvent, EventResult> ScriptLoaded;
        public readonly SystemRequest<ErgoScriptingSystem, ScriptUnloadedEvent, EventResult> ScriptUnloaded;
        public readonly SystemEvent<ErgoScriptingSystem, InputAvailableEvent> InputAvailable;

        private readonly ConcurrentDictionary<string, Script> Cache = new();

        public void ResetPipes()
        {
            In.Writer.Complete();
            In.Reader.Complete();
            Out.Writer.Complete();
            Out.Reader.Complete();
            In.Reset();
            Out.Reset();
        }

        public ErgoScriptingSystem(EventBus bus, IServiceFactory sp, IAsyncInputReader reader) : base(bus)
        {
            OutWriter = TextWriter.Synchronized(new StreamWriter(Out.Writer.AsStream(), Encoding));
            OutReader = TextReader.Synchronized(new StreamReader(Out.Reader.AsStream(), Encoding));
            InWriter = TextWriter.Synchronized(new StreamWriter(In.Writer.AsStream(), Encoding));
            InReader = TextReader.Synchronized(new StreamReader(In.Reader.AsStream(), Encoding));
            AsyncInputReader = reader;
            FieroLib = new(sp);
            Facade = GetErgoFacade();
            Shell = Facade.BuildShell();
            Interpreter = Shell.Interpreter;
            StdlibScope = Interpreter.CreateScope()
                .WithSearchDirectory(@".\Resources\Scripts\");
            var fiero = Interpreter.Load(ref StdlibScope, FieroModule)
                .GetOrThrow(new InvalidOperationException())
                .WithLinkedLibrary(FieroLib);
            var stdlib = StdlibScope.Modules[WellKnown.Modules.Stdlib];
            StdlibScope = StdlibScope
                .WithModule(fiero)
                .WithModule(stdlib
                    .WithImport(FieroModule))
                .WithCurrentModule(WellKnown.Modules.User)
                .WithModule(new Module(WellKnown.Modules.User, runtime: true)
                    .WithImport(WellKnown.Modules.Stdlib));
            ScriptLoaded = new(this, nameof(ScriptLoaded));
            ScriptUnloaded = new(this, nameof(ScriptUnloaded));
            InputAvailable = new(this, nameof(InputAvailable));
        }

        /// <summary>
        /// Configures the Ergo environment before the interpreter is created.
        /// </summary>
        protected virtual ErgoFacade GetErgoFacade()
        {
            return ErgoFacade.Standard
                .AddLibrary(() => FieroLib)
                .SetOutput(OutWriter)
                .SetInput(InReader, Maybe.Some(AsyncInputReader))
                ;
        }

        public bool LoadScript(Script script)
        {
            if (script.IsInvalid())
                return false;
            var localScope = StdlibScope
                .WithRuntime(true)
                .WithExceptionHandler(new ExceptionHandler(@catch: ex =>
                {
                    script.ScriptProperties.LastError = ex;
                    Shell.WriteLine(ex.Message, Ergo.Shell.LogLevel.Err);
                }));
            if (Cache.TryGetValue(script.ScriptProperties.ScriptPath, out var cached))
            {
                Init(script, cached.ScriptProperties.Solver, cached.ScriptProperties.Scope.InterpreterScope);
                return true;
            }
            if (Interpreter.Load(ref localScope, new Atom(script.ScriptProperties.ScriptPath))
                .TryGetValue(out _))
            {
                var solver = Facade.BuildSolver(
                    localScope.BuildKnowledgeBase(),
                    SolverFlags.Default
                );
                Init(script, solver, localScope);
                Cache.TryAdd(script.ScriptProperties.ScriptPath, script);
                //_unload += () => UnloadScript(script);
                return true;
            }
            return false;

            void Init(Script script, ErgoSolver solver, InterpreterScope scope)
            {
                var solverScope = solver.CreateScope(scope);
                if (script.ScriptProperties.ShowTrace)
                {
                    solverScope.Tracer.Trace += Tracer_Trace;
                }
                script.ScriptProperties.Solver = solver;
                script.ScriptProperties.Scope = solverScope;
                // Scripts subscribe to events via the subscribe/2 directive
                if (!FieroLib.GetScriptSubscriptions(script).TryGetValue(out var subbedEvents))
                    subbedEvents = Enumerable.Empty<Signature>();
                // Effects can then read this list and bind the subbed events
                script.ScriptProperties.SubscribedEvents.AddRange(subbedEvents);
                solver.Initialize(localScope);
                ScriptLoaded.Handle(new(script));
            }

            void Tracer_Trace(Tracer _, SolverScope __, SolverTraceType type, string trace)
            {
                Shell.WriteLine(trace, Ergo.Shell.LogLevel.Trc, type);
            }
        }

        public void UnloadAllScripts()
        {
            _unload?.Invoke();
            Cache.Clear();
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
                    var hook = new Hook(new(reqName, 1, sysName, default));
                    finalDict.Add(new(reqName, 1, sysName, default), (self, systems) =>
                    {
                        return ((ISystemRequest)field.GetValue(sys.GetValue(systems)))
                            .SubscribeResponse(evt => Respond(self, evt, reqType, hook));
                    });
                }
                else
                {
                    var evtName = new Atom(field.Name.Replace("Event", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .ToErgoCase());
                    var evtType = field.FieldType.GetGenericArguments()[1];
                    var hook = new Hook(new(evtName, 1, sysName, default));
                    finalDict.Add(new(evtName, 1, sysName, default), (self, systems) =>
                    {
                        return ((ISystemEvent)field.GetValue(sys.GetValue(systems)))
                            .SubscribeHandler(evt => Respond(self, evt, evtType, hook));
                    });
                }
            }
            return finalDict;


            static EventResult Respond(ScriptEffect self, object evt, Type type, Hook hook)
            {
                var term = TermMarshall.ToTerm(evt, type, mode: TermMarshalling.Named);
                try
                {
                    // TODO: Figure out a way for scripts to return complex EventResults?
                    foreach (var ctx in self.Contexts.Values)
                    {
                        var scope = self.Script.ScriptProperties.Scope
                            .WithInterpreterScope(ctx.Scope);
                        if (hook.IsDefined(ctx))
                        {
                            foreach (var _ in hook.Call(ctx, scope, ImmutableArray.Create(term)))
                            {
                                if (self.Script.ScriptProperties.LastError != null)
                                    return false;
                            }
                        }
                    }
                    return true;
                }
                catch (ErgoException ex)
                {
                    // TODO: Log to the script stderr
                    self.Script.ScriptProperties.LastError = ex;
                    return false;
                }
            }
        }
    }
}
