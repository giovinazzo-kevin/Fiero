using Fiero.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Fiero.Business
{
    public class LogComponent : EcsComponent
    {
        protected readonly GameLocalizations<LocaleName> Localizations;
        protected readonly List<string> Messages;

        public IEnumerable<string> GetMessages() => Messages;
        public event Action<LogComponent, string> LogAdded;
        public event Action<LogComponent, string> LogPruned;

        public LogComponent(GameLocalizations<LocaleName> localizations)
        {
            Localizations = localizations;
            Messages = new List<string>();
        }

        public void Write(string message)
        {
            var last = Messages.LastOrDefault();
            foreach (var match in Regex.Matches(message, "\\$(?<key>.*?)\\$").Cast<Match>())
            {
                var translated = Localizations.Get(match.Groups["key"].Value);
                message = message.Replace(match.Value, translated);
            }
            if (last != null)
            {
                var repeatMatch = Regex.Match(last, "x(\\d+)$");
                var repeatCount = 1;
                if (repeatMatch.Success)
                {
                    repeatCount = int.Parse(repeatMatch.Groups[1].Value);
                }
                if (message.Equals(Regex.Replace(last, " x(\\d+)$", String.Empty)))
                {
                    Messages.RemoveAt(Messages.Count - 1);
                    message += $" x{repeatCount + 1}";
                }
            }
            LogAdded?.Invoke(this, message);
            Messages.Add(message);
            if (Messages.Count >= 100)
            {
                LogPruned?.Invoke(this, Messages[0]);
                Messages.RemoveAt(0);
            }
        }
    }
}
