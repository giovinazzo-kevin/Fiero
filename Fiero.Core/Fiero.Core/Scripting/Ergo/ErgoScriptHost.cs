using Ergo.Facade;
using Ergo.Interpreter;
using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Exceptions;
using Ergo.Lang.Extensions;
using Ergo.Shell;
using System.IO.Pipelines;

namespace Fiero.Core
{
    public partial class ErgoScriptHost<TScripts> : IScriptHost<TScripts>
        where TScripts : struct, Enum
    {
        public const string SearchPath = @".\Resources\Scripts\";
        public readonly ErgoFacade Facade;

        public readonly Pipe In = new(), Out = new();
        public readonly TextWriter InWriter, OutWriter;
        public readonly TextReader InReader, OutReader;
        public readonly Encoding Encoding = Encoding.GetEncoding(437);
        public readonly IAsyncInputReader AsyncInputReader;

        public readonly ErgoInterpreter Interpreter;
        public readonly InterpreterScope CoreScope;

        public ErgoScriptHost(IAsyncInputReader inputReader)
        {
            OutWriter = TextWriter.Synchronized(new StreamWriter(Out.Writer.AsStream(), Encoding));
            OutReader = TextReader.Synchronized(new StreamReader(Out.Reader.AsStream(), Encoding));
            InWriter = TextWriter.Synchronized(new StreamWriter(In.Writer.AsStream(), Encoding));
            InReader = TextReader.Synchronized(new StreamReader(In.Reader.AsStream(), Encoding));
            AsyncInputReader = inputReader;
            Facade = GetErgoFacade();
            Interpreter = Facade.BuildInterpreter(InterpreterFlags.Default);
            CoreScope = Interpreter.CreateScope()
                .WithSearchDirectory(SearchPath);
            CoreScope = CoreScope
                .WithModule(Interpreter.Load(ref CoreScope, ErgoModules.Core)
                    .GetOrThrow())
                .WithModule(new Module(WellKnown.Modules.User, runtime: true)
                    .WithImport(ErgoModules.Core))
                .WithCurrentModule(WellKnown.Modules.User)
                .WithBaseModule(ErgoModules.Core);
        }

        protected virtual ErgoFacade GetErgoFacade()
        {
            return ErgoFacade.Standard
                .AddLibrary(() => new CoreLib())
                .SetInput(InReader, Maybe.Some(AsyncInputReader))
                .SetOutput(OutWriter)
                ;
        }
        public virtual bool TryLoad(TScripts fileName, out Script script)
        {
            script = default;
            var localScope = CoreScope;
            if (!Interpreter.Load(ref localScope, new Atom(fileName.ToString().ToErgoCase()))
                .HasValue)
                return false;
            script = new ErgoScript(localScope);
            return true;
        }

        public bool Respond(Script sender, Script.EventHook @event, object payload)
        {
            if (sender is not ErgoScript ergoScript || payload is null)
                return true;
            var type = payload.GetType();
            if (!ergoScript.MarshallingContext.TryGetCached(TermMarshalling.Named, payload, type, default, out var term))
                term = TermMarshall.ToTerm(payload, type, mode: TermMarshalling.Named, ctx: ergoScript.MarshallingContext);
            if (!ergoScript.ErgoHooks.TryGetValue(@event, out var hook))
            {
                var evtName = new Atom(@event.Event.ToErgoCase());
                var sysName = new Atom(@event.System.ToErgoCase());
                hook = ergoScript.ErgoHooks[@event] = new(new(evtName, 1, sysName, default));
            }
            hook.SetArg(0, term);
            var scope = ergoScript.VM.ScopedInstance();
            try
            {
                scope.Query = hook.Compile(throwIfNotDefined: true);
                scope.Run();
                return true;
            }
            catch (ErgoException)
            {
                return false;
            }
        }
    }
}
