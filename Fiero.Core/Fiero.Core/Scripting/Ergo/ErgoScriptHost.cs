using Ergo.Facade;
using Ergo.Interpreter;
using Ergo.Interpreter.Libraries;
using Ergo.Lang;
using Ergo.Lang.Ast;
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

        public readonly ErgoShell Shell;
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
            Shell = Facade.BuildShell(encoding: Encoding);
            Interpreter = Shell.Interpreter;
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
            if (!ergoScript.ErgoEventHooks.TryGetValue(@event, out var hookDef))
            {
                var evtName = new Atom(@event.Event.ToErgoCase());
                var sysName = new Atom(@event.System.ToErgoCase());
                var ergoHook = new Hook(new(evtName, 1, sysName, default));
                hookDef = ergoScript.ErgoEventHooks[@event] = (ergoHook, ergoHook.Compile(throwIfNotDefined: true));
            }
            hookDef.Hook.SetArg(0, term);
            var scope = ergoScript.VM.ScopedInstance();
            scope.Query = hookDef.Op;
            scope.Run();
            return true;
        }

        public bool Observe(Script sender, GameDataStore store, Script.DataHook datum, object oldValue, object newValue)
        {
            if (sender is not ErgoScript ergoScript)
                return true;
            var type = store.GetRegisteredDatumType(datum.Name).T;
            if (!ergoScript.MarshallingContext.TryGetCached(TermMarshalling.Named, oldValue, type, default, out var oldTerm))
                oldTerm = TermMarshall.ToTerm(oldValue, type, mode: TermMarshalling.Named, ctx: ergoScript.MarshallingContext);
            if (!ergoScript.MarshallingContext.TryGetCached(TermMarshalling.Named, newValue, type, default, out var newTerm))
                newTerm = TermMarshall.ToTerm(newValue, type, mode: TermMarshalling.Named, ctx: ergoScript.MarshallingContext);
            if (!ergoScript.ErgoDataHooks.TryGetValue(datum, out var hookDef))
            {
                var datumName = new Atom(datum.Name.ToErgoCase() + "_changed"); // e.g. data:player_name_changed(OldValue, NewValue)
                var ergoHook = new Hook(new(datumName, 2, ErgoModules.Data, default));
                hookDef = ergoScript.ErgoDataHooks[datum] = (ergoHook, ergoHook.Compile(throwIfNotDefined: true));
            }
            hookDef.Hook.SetArg(0, oldTerm);
            hookDef.Hook.SetArg(1, newTerm);
            var scope = ergoScript.VM.ScopedInstance();
            scope.Query = hookDef.Op;
            scope.Run();
            return true;
        }
    }
}
