using SFML.Window;
using System.Text.RegularExpressions;


namespace Fiero.Core
{
    public interface IScriptHost
    {
        bool TryLoad(string fileName, out Script script);
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
            var sysRegex = new Regex("System$", RegexOptions.IgnoreCase);
            var reqRegex = new Regex("Request$", RegexOptions.IgnoreCase);
            var evtRegex = new Regex("Event$", RegexOptions.IgnoreCase);
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
    }

    public partial interface IScriptHost<TScript> : IScriptHost
        where TScript : Script
    {
        bool TryLoad(string fileName, out TScript script)
        {
            if(((IScriptHost)this).TryLoad(fileName, out var script_))
            {
                script = (TScript)script_;
                return true;
            }
            script = default;
            return false;
        }
    }
}
