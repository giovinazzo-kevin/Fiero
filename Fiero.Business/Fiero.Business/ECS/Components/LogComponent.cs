using Fiero.Core;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Fiero.Business
{
    public class LogComponent : Component
    {
        protected readonly GameLocalizations<LocaleName> Localizations;
        protected readonly List<string> Messages;

        public IEnumerable<string> GetMessages() => Messages;

        public LogComponent(GameLocalizations<LocaleName> localizations)
        {
            Localizations = localizations;
            Messages = new List<string>();
        }

        public void Write(string message)
        {
            foreach (var match in Regex.Matches(message, "\\$(?<key>.*?)\\$").Cast<Match>()) {
                var translated = Localizations.Get(match.Groups["key"].Value);
                message = message.Replace(match.Value, translated);
            }
            Messages.Add(message);
            if(Messages.Count >= 100) {
                Messages.RemoveAt(0);
            }
        }
    }
}
