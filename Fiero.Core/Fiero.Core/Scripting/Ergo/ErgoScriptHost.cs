using Ergo.Facade;
using Ergo.Interpreter;
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
                .WithModule(Interpreter.Load(ref CoreScope, Modules.Core)
                    .GetOrThrow())
                .WithModule(new Module(WellKnown.Modules.User, runtime: true)
                    .WithImport(Modules.Core))
                .WithCurrentModule(WellKnown.Modules.User)
                .WithBaseModule(Modules.Core);
        }

        protected virtual ErgoFacade GetErgoFacade()
        {
            return ErgoFacade.Standard
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
    }
}
