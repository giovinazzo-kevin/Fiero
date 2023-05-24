using Ergo.Shell;
using Fiero.Core;
using System;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using Unconcern;
using Unconcern.Common;

namespace Fiero.Business
{
    [TransientDependency]
    public partial class DeveloperConsole : Widget
    {
        public const double ScriptUpdateRate = 0.15;
        public const int TabSize = 2;

        private readonly StringBuilder _inputBuffer = new();

        protected readonly GameColors<ColorName> Colors;
        protected Textbox InputTextbox { get; private set; }
        protected ConsolePane Pane { get; private set; }

        public readonly EventBus EventBus;
        public readonly ErgoScriptingSystem ScriptingSystem;

        public event Action<DeveloperConsole, string> OutputAvailable;
        public event Action<DeveloperConsole, string> InputAvailable;

        public Coord Cursor { get; set; }

        public DeveloperConsole(ErgoScriptingSystem scripting, EventBus bus, GameUI ui, GameColors<ColorName> colors)
            : base(ui)
        {
            EventBus = bus;
            Colors = colors;
            ScriptingSystem = scripting;
            OutputAvailable += OnOutputAvailable;
            InputAvailable += OnInputAvailable;
        }

        protected void OnInputAvailable(DeveloperConsole self, string chunk)
        {
            //ScriptingSystem.InputAvailable.Raise(new(chunk));
            Pane.WriteLine(string.Empty);
        }

        public void Put(char c)
        {
            switch (c)
            {
                case '\n':
                    _inputBuffer.Append(c);
                    InputAvailable?.Invoke(this, _inputBuffer.ToString());
                    _inputBuffer.Clear();
                    Cursor += Coord.PositiveY;
                    break;
                case '\b' when _inputBuffer.Length > 0:
                    Cursor -= Coord.PositiveX;
                    break;
                case '\r':
                    Cursor *= Coord.PositiveY;
                    break;
                case '\t':
                    for (int i = 0; i < TabSize; i++)
                        Put(' ');
                    break;
                default:
                    _inputBuffer.Insert(Cursor.X, c);
                    Cursor += Coord.PositiveX;
                    if (Cursor.X < _inputBuffer.Length)
                        _inputBuffer.Remove(Cursor.X, 1);
                    break;
            }
        }

        public void Write(string s)
        {
            foreach (char c in s)
                Put(c);
        }

        public void WriteLine(string s)
        {
            Write(s + Environment.NewLine);
        }

        public void Show()
        {
            Layout.IsHidden.V = false;
            Layout.Focus(InputTextbox);
        }

        public void Hide()
        {
            Layout.IsHidden.V = true;
            InputTextbox.IsActive.V = false;
        }

        protected virtual void OnOutputAvailable(DeveloperConsole self, string chunk)
        {
            if (Layout is null)
                return;
            Pane.Write(chunk);
        }

        public Subscription TrackShell(ErgoShell s, bool routeStdin = true)
        {
            s.UseColors = false;
            s.UseANSIEscapeSequences = false;
            s.UseUnicodeSymbols = false;
            var outPipe = new Pipe();
            var outWriter = TextWriter.Synchronized(new StreamWriter(outPipe.Writer.AsStream(), s.Encoding));
            var outReader = TextReader.Synchronized(new StreamReader(outPipe.Reader.AsStream(), s.Encoding));
            var inPipe = new Pipe();
            var inWriter = TextWriter.Synchronized(new StreamWriter(inPipe.Writer.AsStream(), s.Encoding));
            var inReader = TextReader.Synchronized(new StreamReader(inPipe.Reader.AsStream(), s.Encoding));
            s.SetOut(outWriter);
            s.SetIn(inReader);
            var cts = new CancellationTokenSource();
            var outExpr = Concern.Defer()
                .UseAsynchronousTimer()
                .Do(async token =>
                {
                    var sb = new StringBuilder();
                    var result = await outPipe.Reader.ReadAsync(token);
                    var buffer = result.Buffer;
                    foreach (var segment in buffer)
                    {
                        var bytes = segment.Span.ToArray();
                        var str = outWriter.Encoding.GetString(bytes);
                        sb.Append(str);
                    }
                    outPipe.Reader.AdvanceTo(buffer.End);
                    OutputAvailable?.Invoke(this, sb.ToString());
                })
                .Build();
            var replExpr = Concern.Defer()
                .UseAsynchronousTimer()
                .Do(async token =>
                {
                    await foreach (var ans in s.Repl(_ => ScriptingSystem.ShellScope))
                    {

                    }
                })
                .Build();
            _ = Concern.Deferral.LoopForever(outExpr, cts.Token);
            _ = Concern.Deferral.LoopForever(replExpr, cts.Token);
            var closure = new ShellClosure(s, inWriter);
            if (routeStdin) InputAvailable += closure.OnInputAvailable;
            return new(new[] { () => {
                cts.Cancel();
                outPipe.Reader.Complete();
                outPipe.Writer.Complete();
                inPipe.Reader.Complete();
                inPipe.Writer.Complete();
            if (routeStdin) InputAvailable -= closure.OnInputAvailable;
            } });
        }

        public Subscription TrackScript(Script s, bool routeStdin = false)
        {
            var inPipe = new Pipe();
            var inWriter = TextWriter.Synchronized(new StreamWriter(inPipe.Writer.AsStream(), s.ScriptProperties.Solver.Out.Encoding));
            var inReader = TextReader.Synchronized(new StreamReader(inPipe.Reader.AsStream(), s.ScriptProperties.Solver.Out.Encoding));
            s.ScriptProperties.In = inWriter;
            s.ScriptProperties.Solver.SetIn(inReader);
            var closure = new ScriptClosure(s);
            if (routeStdin) InputAvailable += closure.OnInputAvailable;
            return new(new[] { () => {
                inPipe.Reader.Complete();
                inPipe.Writer.Complete();
                if(routeStdin) InputAvailable -= closure.OnInputAvailable;
            } });
        }

        protected override LayoutStyleBuilder DefineStyles(LayoutStyleBuilder builder) => base.DefineStyles(builder)
            .AddRule<ConsolePane>(r => r.Apply(p =>
            {
                var ts = UI.Store.Get(Data.UI.TileSize);
                p.Background.V = Colors.Get(ColorName.Black).AddAlpha(-64);
                p.Foreground.V = Colors.Get(ColorName.White);
                p.Padding.V = new(ts, ts);
            }))
            .AddRule<Textbox>(r => r.Apply(t =>
            {
                var ts = UI.Store.Get(Data.UI.TileSize);
                t.Padding.V = new(ts / 2, 0);
                t.MaxLength.V = t.ContentRenderSize.X / t.FontSize.V.X;
                t.Background.V = Colors.Get(ColorName.Black).AddAlpha(-128);
                t.Foreground.V = Colors.Get(ColorName.White);
            }))
            .AddRule<UIControl>(r => r.Apply(x =>
            {
                x.OutlineColor.V = Colors.Get(ColorName.White).AddAlpha(-64);
                x.OutlineThickness.V = 1;
            }))
            ;

        protected override LayoutGrid RenderContent(LayoutGrid grid) => grid
                .Row(id: "output")
                    .Cell<ConsolePane>(p => Pane = p)
                .End()
                .Row(h: 20, px: true, id: "input")
                    .Cell<Textbox>(t =>
                    {
                        InputTextbox = t;
                        t.EnterPressed += obj =>
                        {
                            var text = obj.Text.V;
                            obj.Text.V = string.Empty;
                            WriteLine(text);
                        };
                    })
                .End()
            ;

        public override void Update()
        {
            base.Update();
        }
    }
}
