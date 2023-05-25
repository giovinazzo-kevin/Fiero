using Ergo.Shell;
using System.IO;

namespace Fiero.Business
{
    public partial class DeveloperConsole
    {
        record class ShellClosure(ErgoShell s, TextWriter inWriter)
        {
            public void OnCharAvailable(DeveloperConsole _, char c)
            {
                inWriter.Write(c);
                inWriter.Flush();
            }
            public void OnLineAvailable(DeveloperConsole _, string s)
            {
                inWriter.WriteLine(s);
                inWriter.Flush();
            }
        };
    }
}
