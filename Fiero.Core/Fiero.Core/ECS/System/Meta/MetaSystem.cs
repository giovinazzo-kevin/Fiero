using System.Reflection;
using Unconcern;
using Unconcern.Common;
using Unconcern.Delegation;

namespace Fiero.Core
{
    /// <summary>
    /// Core system that interfaces with all the other systems.
    /// </summary>
    public partial class MetaSystem : EcsSystem
    {
        public record struct SystemEventField(EcsSystem System, FieldInfo Field, bool IsRequest);
        protected readonly Dictionary<Type, EcsSystem> TrackedSystems = new();

        public MetaSystem(EventBus bus) : base(bus)
        {
            Subscriptions.Add(Intercept<SystemCreatedEvent>(x => TrackedSystems.Add(x.System.GetType(), x.System)));
            Subscriptions.Add(Intercept<SystemDisposedEvent>(x => TrackedSystems.Remove(x.System.GetType())));
            Subscription Intercept<T>(Action<T> handle)
            {
                return Concern.Delegate(EventBus)
                    .When<SystemMessage<EcsSystem, T>>(msg => !Equals(msg.Content.Data, this))
                    .Do<SystemMessage<EcsSystem, T>>(msg => handle(msg.Content.Data))
                    .Build()
                    .Listen(EventHubName);
            }
        }

        public T GetSystem<T>() where T : EcsSystem => (T)TrackedSystems[typeof(T)];

        public IEnumerable<SystemEventField> GetSystemEventFields()
        {
            foreach (var (type, s) in TrackedSystems)
            {
                foreach (var f in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (f.FieldType.IsAssignableTo(typeof(ISystemRequest)))
                        yield return new(s, f, true);
                    else if (f.FieldType.IsAssignableTo(typeof(ISystemEvent)))
                        yield return new(s, f, false);
                }
            }
        }


        /// <summary>
        /// Maps every SystemRequest and SystemEvent defined in all systems to a wrapper that
        /// calls an Ergo script automatically and parses its result as an EventResult for Fiero,
        /// returning a subscription that will be disposed when this effect ends.
        /// </summary>
        /// <returns>All routes indexed by signature.</returns>
        //public Dictionary<Signature, Func<Script, Subscription>> GetScriptRoutes()
        //{
        //    var sysRegex = NormalizeSystemName();
        //    var reqRegex = NormalizeRequestName();
        //    var evtRegex = NormalizeEventName();
        //    var finalDict = new Dictionary<Signature, Func<Script, Subscription>>();
        //    foreach (var (sys, field, isReq) in GetSystemEventFields())
        //    {
        //        var sysName = new Atom(sysRegex.Replace(sys.GetType().Name.ToErgoCase(), string.Empty)
        //            .ToErgoCase());
        //        if (isReq)
        //        {
        //            var reqName = new Atom(reqRegex.Replace(field.Name, string.Empty)
        //                .ToErgoCase());
        //            var reqType = field.FieldType.GetGenericArguments()[1];
        //            var hook = new Hook(new(reqName, 1, sysName, default));
        //            var compiledHook = hook.Compile();
        //            finalDict.Add(new(reqName, 1, sysName, default), (self) =>
        //            {
        //                var systemEvent = ((ISystemEvent)field.GetValue(sys));
        //                return ((ISystemRequest)systemEvent)
        //                    .SubscribeResponse(evt =>
        //                    {
        //                        return Respond(self, evt, reqType, hook, compiledHook, systemEvent.MarshallingContext);
        //                    });
        //            });
        //        }
        //        else
        //        {
        //            var evtName = new Atom(evtRegex.Replace(field.Name, string.Empty)
        //                .ToErgoCase());
        //            var evtType = field.FieldType.GetGenericArguments()[1];
        //            var hook = new Hook(new(evtName, 1, sysName, default));
        //            var compiledHook = hook.Compile();
        //            finalDict.Add(new(evtName, 1, sysName, default), (self) =>
        //            {
        //                var systemEvent = ((ISystemEvent)field.GetValue(sys));
        //                return systemEvent
        //                    .SubscribeHandler(evt =>
        //                    {
        //                        Respond(self, evt, evtType, hook, compiledHook, systemEvent.MarshallingContext);
        //                    });
        //            });
        //        }
        //    }
        //    return finalDict;


        //    static object Respond(Script self, object evt, Type type, Hook hook, ErgoVM.Op op, TermMarshallingContext mctx)
        //    {
        //        if (!mctx.TryGetCached(TermMarshalling.Named, evt, type, default, out var term))
        //            term = TermMarshall.ToTerm(evt, type, mode: TermMarshalling.Named, ctx: mctx);
        //        hook.SetArg(0, term);
        //        try
        //        {
        //            var scope = self.

        //            // TODO: Figure out a way for scripts to return complex EventResults?
        //            foreach (var ctx in self.Contexts.Values)
        //            {
        //                var scope = ctx.ScopedInstance();
        //                scope.Query = op;
        //                scope.Run();
        //                if (self.Script.ScriptProperties.LastError != null)
        //                    return false;
        //            }
        //            return true;
        //        }
        //        catch (ErgoException ex)
        //        {
        //            // TODO: Log to the script stderr
        //            self.Script.ScriptProperties.LastError = ex;
        //            return false;
        //        }
        //    }
        //}
    }
}
