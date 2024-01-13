using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using System.Reflection;
using Unconcern;
using Unconcern.Common;
using Unconcern.Delegation;

namespace Fiero.Core
{
    /// <summary>
    /// Core system that interfaces with all the other systems and acts as a central repository.
    /// </summary>
    [SingletonDependency]
    public partial class MetaSystem : EcsSystem
    {
        public record struct SystemEventField(EcsSystem System, FieldInfo Field, bool IsRequest);
        public readonly record struct ScriptEvent(string System, string Event, ITerm Data);
        public readonly record struct ScriptDatumEvent(string Module, string Name, ITerm OldValue, ITerm NewValue);

        protected readonly Dictionary<Type, EcsSystem> TrackedSystems = new();
        protected readonly IServiceFactory ServiceFactory;

        private volatile bool invalidateCache = true;
        private readonly List<SystemEventField> cachedFields = new();

        /// <summary>
        /// Raised when an event that does not exist in any system is raised; allows scripts to define their own events.
        /// </summary>
        public readonly SystemEvent<MetaSystem, ScriptEvent> ScriptEventRaised;
        /// <summary>
        /// Raised when a datum that does not exist in any module is modified; allows scripts to define their own data.
        /// </summary>
        public readonly SystemEvent<MetaSystem, ScriptDatumEvent> ScriptDatumChanged;

        public MetaSystem(EventBus bus, IServiceFactory fac) : base(bus)
        {
            ServiceFactory = fac;
            TrackedSystems.Add(GetType(), this);
            Subscriptions.Add(Intercept<SystemCreatedEvent>(x =>
            {
                TrackedSystems.Add(x.System.GetType(), x.System);
                invalidateCache = true;
            }));
            Subscriptions.Add(Intercept<SystemDisposedEvent>(x =>
            {
                TrackedSystems.Remove(x.System.GetType());
                invalidateCache = true;
            }));
            Subscriptions.Add(Concern.Delegate(EventBus)
                    .When<GameDataStore.DatumChangedEvent>(msg => !msg.Content.Datum.IsStatic)
                    .Do<GameDataStore.DatumChangedEvent>(msg =>
                        _ = ScriptDatumChanged.Raise(new ScriptDatumEvent(msg.Content.Datum.Module.ToErgoCase(), msg.Content.Datum.Name.ToErgoCase(), ((ITerm)msg.Content.OldValue ?? WellKnown.Literals.Discard), (ITerm)msg.Content.NewValue)))
                    .Build()
                    .Listen(GameDataStore.EventHubName));
            ScriptEventRaised = new(this, nameof(ScriptEventRaised));
            ScriptDatumChanged = new(this, nameof(ScriptDatumChanged));
            Subscription Intercept<T>(Action<T> handle)
            {
                return Concern.Delegate(EventBus)
                    .When<SystemMessage<EcsSystem, T>>(msg => true)
                    .Do<SystemMessage<EcsSystem, T>>(msg => handle(msg.Content.Data))
                    .Build()
                    .Listen(EventHubName);
            }
        }

        public void Initialize()
        {
            // Populate TrackedSystems by listening to SystemCreatedEvent
            _ = ServiceFactory.GetAllInstances<EcsSystem>();
        }

        public T Get<T>() where T : EcsSystem => (T)TrackedSystems[typeof(T)];
        public T Resolve<T>() => ServiceFactory.GetInstance<T>();

        public IEnumerable<SystemEventField> GetSystemEventFields()
        {
            if (!invalidateCache)
                return cachedFields;
            cachedFields.Clear();
            foreach (var (type, s) in TrackedSystems)
            {
                foreach (var f in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (f.FieldType.IsAssignableTo(typeof(ISystemRequest)))
                        cachedFields.Add(new(s, f, true));
                    else if (f.FieldType.IsAssignableTo(typeof(ISystemEvent)))
                        cachedFields.Add(new(s, f, false));
                }
            }
            return cachedFields;
        }
    }
}
