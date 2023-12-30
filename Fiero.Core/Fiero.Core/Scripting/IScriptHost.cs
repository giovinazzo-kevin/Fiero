using System.Text.RegularExpressions;


namespace Fiero.Core
{
    public partial interface IScriptHost<TScripts>
        where TScripts : struct, Enum
    {
        bool TryLoad(TScripts fileName, out Script script);

        bool Respond(Script sender, Script.EventHook @event, object payload);
        bool Observe(Script sender, GameDataStore store, Script.DataHook datum, object oldValue, object newValue);

        /// <summary>
        /// Generates a list of routes that can be used to let a script observe a stream of changing values from all the game data types registered in the store.
        /// </summary>
        public ScriptDataRoutes GetScriptDataRoutes(GameDataStore store)
        {
            var finalDict = new ScriptDataRoutes();
            foreach (var datum in store.GetRegisteredDatumTypes())
            {
                var dataHook = new Script.DataHook(datum.Module, datum.Name);
                finalDict.Add(dataHook, (script) => store.SubscribeHandler(datum.Module, datum.Name, msg => Observe(script, store, dataHook, msg.OldValue, msg.NewValue)));
            }
            return finalDict;
        }

        /// <summary>
        /// Creates a list of routes that can be used to let a script react to the events sent by the various game systems.
        /// </summary>
        public ScriptEventRoutes GetScriptEventRoutes(MetaSystem meta)
        {
            var sysRegex = NormalizeSystemName();
            var reqRegex = NormalizeRequestName();
            var evtRegex = NormalizeEventName();
            var finalDict = new ScriptEventRoutes();
            foreach (var (sys, field, isReq) in meta.GetSystemEventFields())
            {
                var sysName = sysRegex.Replace(sys.GetType().Name, string.Empty);
                var systemEvent = ((ISystemEvent)field.GetValue(sys));
                if (isReq)
                {
                    var eventHook = new Script.EventHook(sysName, reqRegex.Replace(field.Name, string.Empty));
                    finalDict.Add(eventHook, (script) =>
                    {
                        return ((ISystemRequest)systemEvent)
                            .SubscribeResponse(evt => Respond(script, eventHook, evt));
                    });
                }
                else
                {
                    var eventHook = new Script.EventHook(sysName, reqRegex.Replace(field.Name, string.Empty));
                    finalDict.Add(eventHook, (script) =>
                    {
                        return systemEvent
                            .SubscribeHandler(evt => Respond(script, eventHook, evt));
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
