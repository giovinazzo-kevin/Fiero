using Unconcern.Common;

namespace Fiero.Core
{
    /// <summary>
    /// Handles caching of scripts and their top-level routing through a provided IScriptHost.
    /// </summary>
    public class GameScripts<TScripts>(IScriptHost<TScripts> host, MetaSystem meta, GameDataStore store)
        where TScripts : struct, Enum
    {
        protected readonly Dictionary<string, Script> Scripts = new();
        public readonly IScriptHost<TScripts> Host = host;

        private static string CacheKey(TScripts a, string b) => $"{a}{b}";

        public bool TryLoad(TScripts key, out Script script, string cacheKey = null)
        {
            if (Host.TryLoad(key, out script))
            {
                Scripts[CacheKey(key, cacheKey)] = script;
                return true;
            }
            return false;
        }
        public bool TryGet(TScripts key, out Script script, string cacheKey = null) => Scripts.TryGetValue(CacheKey(key, cacheKey), out script);
        public Script Get(TScripts key, string cacheKey = null) => Scripts[CacheKey(key, cacheKey)];

        public IEnumerable<Subscription> RouteSubscriptions()
        {
            var eventRoutes = Host.GetScriptEventRoutes(meta);
            var dataRoutes = Host.GetScriptDataRoutes(store);
            foreach (var item in Scripts.Values)
                yield return item.Run(eventRoutes, dataRoutes);
        }
    }
}
