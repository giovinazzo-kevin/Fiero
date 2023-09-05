using Ergo.Shell;
using System.IO;

namespace Fiero.Business
{
    public partial class DeveloperConsole
    {
        record class ShellClosure(ErgoShell Shell, TextWriter InWriter)
        {
            public void OnCharAvailable(DeveloperConsole _, char c)
            {
                InWriter.Write(c);
                InWriter.Flush();
            }
            public void OnLineAvailable(DeveloperConsole _, string s)
            {
                InWriter.WriteLine(s);
                InWriter.Flush();
            }
        };
    }
}
