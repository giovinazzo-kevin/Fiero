using Ergo.Shell;
using System.IO;
using System.Text.RegularExpressions;

namespace Fiero.Business
{
    public partial class DeveloperConsole
    {
        record class ShellClosure(ErgoShell s, TextWriter inWriter)
        {
            protected readonly Regex QueryRegex = new(@"^\s*(.*?)\s*\.\s*\n$");

            public void OnInputAvailable(DeveloperConsole _, string chunk)
            {
                if (QueryRegex.Match(chunk) is { Success: true, Groups: var groups })
                {
                    var query = groups[1].Value;
                    inWriter.WriteLine(query);
                    inWriter.Flush();
                }
            }
        };
    }
}
