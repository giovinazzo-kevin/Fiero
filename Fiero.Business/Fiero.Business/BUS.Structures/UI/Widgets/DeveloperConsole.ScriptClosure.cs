using System.IO;
using System.Text.RegularExpressions;

namespace Fiero.Business
{
    public partial class DeveloperConsole
    {
        record class ScriptClosure(Script s)
        {
            static string ScriptName(Script s) => Path.GetFileNameWithoutExtension(s.ScriptProperties.ScriptPath);

            protected readonly Regex InputRegex = new($@"^\s*:{ScriptName(s)}\s*");

            public void OnInputAvailable(DeveloperConsole _, string chunk)
            {
                if (InputRegex.IsMatch(chunk))
                {
                    chunk = InputRegex.Replace(chunk, string.Empty);
                    s.ScriptProperties.In.Write(chunk);
                    s.ScriptProperties.In.Flush();
                }
            }
        };
    }
}
