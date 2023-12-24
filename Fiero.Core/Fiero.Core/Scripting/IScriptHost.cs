using System.Text.RegularExpressions;


namespace Fiero.Core
{
    public partial interface IScriptHost<TScripts>
        where TScripts : struct, Enum
    {
        bool TryLoad(TScripts fileName, out Script script);

        bool Respond(Script sender, Script.EventHook @event, object payload);


        public ScriptRoutes GetScriptRoutes(MetaSystem meta)
        {
            var sysRegex = NormalizeSystemName();
            var reqRegex = NormalizeRequestName();
            var evtRegex = NormalizeEventName();
            var finalDict = new ScriptRoutes();
            foreach (var (sys, field, isReq) in meta.GetSystemEventFields())
            {
                var sysName = sysRegex.Replace(sys.GetType().Name, string.Empty);
                var systemEvent = ((ISystemEvent)field.GetValue(sys));
                if (isReq)
                {
                    var eventHook = new Script.EventHook(sysName, reqRegex.Replace(field.Name, string.Empty));
                    finalDict.Add(eventHook, (self) =>
                    {
                        return ((ISystemRequest)systemEvent)
                            .SubscribeResponse(evt => Respond(self, eventHook, evt));
                    });
                }
                else
                {
                    var eventHook = new Script.EventHook(sysName, reqRegex.Replace(field.Name, string.Empty));
                    finalDict.Add(eventHook, (self) =>
                    {
                        return systemEvent
                            .SubscribeHandler(evt => Respond(self, eventHook, evt));
                    });
                }
            }
            return finalDict;
        }


        [GeneratedRegex("System$", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex NormalizeSystemName();
        [GeneratedRegex("Request$", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex NormalizeRequestName();
        [GeneratedRegex("Event$", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex NormalizeEventName();
    }
}
