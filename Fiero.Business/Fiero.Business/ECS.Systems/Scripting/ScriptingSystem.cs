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
using System.Text.RegularExpressions;
using Unconcern.Common;

namespace Fiero.Business
{
    public partial class ScriptingSystem : EcsSystem
    {
        private const string ScriptsPath = @".\Resources\Scripts\";
        public static readonly Atom FieroModule = new("fiero");
        public static readonly Atom AnimationModule = new("anim");
        public static readonly Atom SoundModule = new("sound");
        public static readonly Atom EffectModule = new("effect");
        public static readonly Atom DataModule = new("data");
        public static readonly Atom EventModule = new("event");
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
        private string[] _visibleScripts;

        public readonly Encoding Encoding = Encoding.GetEncoding(437);
        public readonly SystemRequest<ScriptingSystem, ScriptLoadedEvent, EventResult> ScriptLoaded;
        public readonly SystemRequest<ScriptingSystem, ScriptUnloadedEvent, EventResult> ScriptUnloaded;
        public readonly SystemEvent<ScriptingSystem, ScriptEventRaisedEvent> ScriptEventRaised;
        public readonly ConcurrentDictionary<string, Script> Cache = new();

        public ScriptingSystem(EventBus bus, IServiceFactory sp, IAsyncInputReader reader) : base(bus)
        {
            OutWriter = TextWriter.Synchronized(new StreamWriter(Out.Writer.AsStream(), Encoding));
            OutReader = TextReader.Synchronized(new StreamReader(Out.Reader.AsStream(), Encoding));
            InWriter = TextWriter.Synchronized(new StreamWriter(In.Writer.AsStream(), Encoding));
            InReader = TextReader.Synchronized(new StreamReader(In.Reader.AsStream(), Encoding));
            AsyncInputReader = reader;
            FieroLib = new(sp);
            Facade = GetErgoFacade(sp);
            Shell = Facade.BuildShell();
            Interpreter = Shell.Interpreter;
            StdlibScope = Interpreter.CreateScope()
                .WithSearchDirectory(ScriptsPath);
            var fiero = Interpreter.Load(ref StdlibScope, FieroModule)
                .GetOrThrow(new InvalidOperationException());
            StdlibScope = StdlibScope
                .WithModule(fiero)
                .WithModule(new Module(WellKnown.Modules.User, runtime: true)
                    .WithImport(FieroModule))
                .WithCurrentModule(WellKnown.Modules.User)
                .WithBaseModule(FieroModule);
            ScriptLoaded = new(this, nameof(ScriptLoaded), asynchronous: true);
            ScriptUnloaded = new(this, nameof(ScriptUnloaded), asynchronous: true);
            ScriptEventRaised = new(this, nameof(ScriptEventRaised), asynchronous: false);
            _visibleScripts = Directory.EnumerateFiles(ScriptsPath, "*.ergo", SearchOption.AllDirectories)
                .Select(Path.GetFileNameWithoutExtension)
                .ToArray();
        }

        /// <summary>
        /// Configures the Ergo environment before the interpreter is created.
        /// </summary>
        protected virtual ErgoFacade GetErgoFacade(IServiceFactory sp)
        {
            return ErgoFacade.Standard
                .AddLibrary(() => FieroLib)
                .SetOutput(OutWriter)
                .SetInput(InReader, Maybe.Some(AsyncInputReader))
                .AddCommand(sp.GetInstance<SelectScript>())
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
            var cacheKey = $"{script.ScriptProperties.ScriptPath}{script.ScriptProperties.CacheKey}";
            if (script.ScriptProperties.Cached && Cache.TryGetValue(cacheKey, out var cached))
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
                solver.Initialize(localScope);
                Init(script, solver, localScope);
                Cache.TryAdd(cacheKey, script);
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
                ScriptLoaded.Handle(new(script));

                void Tracer_Trace(Tracer _, SolverScope scope, SolverTraceType type, string trace)
                {
                    Shell.WriteLine(trace, Ergo.Shell.LogLevel.Trc, type);
                }
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

        public IEnumerable<string> GetVisibleScripts() => _visibleScripts;

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

            var sysRegex = NormalizeSystemName();
            var reqRegex = NormalizeRequestName();
            var evtRegex = NormalizeEventName();

            var finalDict = new Dictionary<Signature, Func<ScriptEffect, GameSystems, Subscription>>();
            foreach (var (sys, field, isReq) in MetaSystem.GetSystemEventFields())
            {
                var sysName = new Atom(sysRegex.Replace(sys.Name, string.Empty)
                    .ToErgoCase());
                if (isReq)
                {
                    var reqName = new Atom(reqRegex.Replace(field.Name, string.Empty)
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
                    var evtName = new Atom(evtRegex.Replace(field.Name, string.Empty)
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

        [GeneratedRegex("System$", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex NormalizeSystemName();
        [GeneratedRegex("Request$", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex NormalizeRequestName();
        [GeneratedRegex("Event$", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex NormalizeEventName();
    }
}
