﻿using Ergo.Facade;
using Ergo.Interpreter;
using Ergo.Interpreter.Libraries;
using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using Ergo.Shell;
using Fiero.Core.Ergo.Libraries.Core;
using System.IO.Pipelines;

namespace Fiero.Core.Ergo
{
    public abstract class ErgoScriptHostBase
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

        protected readonly IServiceFactory ServiceFactory;


        public ErgoScriptHostBase(IAsyncInputReader inputReader, IServiceFactory fac)
        {
            ServiceFactory = fac;
            OutWriter = TextWriter.Synchronized(new StreamWriter(Out.Writer.AsStream(), Encoding));
            OutReader = TextReader.Synchronized(new StreamReader(Out.Reader.AsStream(), Encoding));
            InWriter = TextWriter.Synchronized(new StreamWriter(In.Writer.AsStream(), Encoding));
            InReader = TextReader.Synchronized(new StreamReader(In.Reader.AsStream(), Encoding));
            AsyncInputReader = inputReader;
            Facade = GetErgoFacade();
            Shell = Facade.BuildShell(encoding: Encoding);
            Interpreter = Shell.Interpreter;
            CoreScope = GetCoreScope();
        }

        protected virtual InterpreterScope GetCoreScope()
        {
            var coreScope = Interpreter.CreateScope()
                .WithSearchDirectory(SearchPath)
                .WithExceptionHandler(Shell.LoggingExceptionHandler);
            coreScope = coreScope
                .WithModule(Interpreter.Load(ref coreScope, CoreErgoModules.Core)
                    .GetOrThrow())
                .WithModule(new Module(WellKnown.Modules.User, runtime: true)
                    .WithImport(CoreErgoModules.Core))
                .WithCurrentModule(WellKnown.Modules.User)
                .WithBaseModule(CoreErgoModules.Core);
            return coreScope;
        }

        protected virtual ErgoFacade GetErgoFacade()
        {
            return ErgoFacade.Standard
                .AddLibrary(() => ServiceFactory.GetInstance<CoreLib>())
                .SetInput(InReader, Maybe.Some(AsyncInputReader))
                .SetOutput(OutWriter)
                ;
        }

        protected abstract ErgoScript MakeScript(InterpreterScope scope);

        public bool TryLoad(string fileName, out Script script)
        {
            script = default;
            var localScope = CoreScope;
            if (!Interpreter.Load(ref localScope, new Atom(fileName.ToString().ToErgoCase()))
                .HasValue)
                return false;
            script = MakeScript(localScope);
            return true;
        }

        public bool Respond(Script sender, Script.EventHook @event, object payload)
        {
            if (sender is not ErgoScript script)
                return true;
            if (payload is null)
                return true;
            var type = payload.GetType();
            if (!script.MarshallingContext.TryGetCached(TermMarshalling.Named, payload, type, default, out var term))
                term = TermMarshall.ToTerm(payload, type, mode: TermMarshalling.Named, ctx: script.MarshallingContext);
            if (!script.ErgoEventHooks.TryGetValue(@event, out var hookDef))
            {
                var evtName = new Atom(@event.Event.ToErgoCase());
                var sysName = new Atom(@event.System.ToErgoCase());
                var ergoHook = new Hook(new(evtName, 1, sysName, default));
                hookDef = script.ErgoEventHooks[@event] = (ergoHook, ergoHook.Compile(throwIfNotDefined: true));
            }
            hookDef.Hook.SetArg(0, term);
            var scope = script.VM.ScopedInstance();
            scope.Query = hookDef.Op;
            scope.Run();
            return true;
        }

        public bool Observe(Script sender, GameDataStore store, Script.DataHook datum, object oldValue, object newValue)
        {
            if (sender is not ErgoScript script)
                return true;
            var type = store.GetRegisteredDatumType(datum.Module, datum.Name).T;
            if (!script.MarshallingContext.TryGetCached(TermMarshalling.Named, oldValue, type, default, out var oldTerm))
                oldTerm = TermMarshall.ToTerm(oldValue, type, mode: TermMarshalling.Named, ctx: script.MarshallingContext);
            if (!script.MarshallingContext.TryGetCached(TermMarshalling.Named, newValue, type, default, out var newTerm))
                newTerm = TermMarshall.ToTerm(newValue, type, mode: TermMarshalling.Named, ctx: script.MarshallingContext);
            if (!script.ErgoDataHooks.TryGetValue(datum, out var hookDef))
            {
                var datumName = new Atom(datum.Name.ToErgoCase() + "_changed"); // e.g. data:player_name_changed(OldValue, NewValue)
                var ergoHook = new Hook(new(datumName, 2, new Atom(datum.Module.ToErgoCase()), default));
                hookDef = script.ErgoDataHooks[datum] = (ergoHook, ergoHook.Compile(throwIfNotDefined: true));
            }
            hookDef.Hook.SetArg(0, oldTerm);
            hookDef.Hook.SetArg(1, newTerm);
            var scope = script.VM.ScopedInstance();
            scope.Query = hookDef.Op;
            scope.Run();
            return true;
        }
    }
}
