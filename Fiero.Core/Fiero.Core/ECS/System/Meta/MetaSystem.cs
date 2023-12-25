using Ergo.Lang.Ast;
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

        protected readonly Dictionary<Type, EcsSystem> TrackedSystems = new();
        protected readonly IServiceFactory ServiceFactory;

        public readonly SystemEvent<MetaSystem, ScriptEvent> ScriptEventRaised;

        public MetaSystem(EventBus bus, IServiceFactory fac) : base(bus)
        {
            ServiceFactory = fac;
            TrackedSystems.Add(GetType(), this);
            Subscriptions.Add(Intercept<SystemCreatedEvent>(x => TrackedSystems.Add(x.System.GetType(), x.System)));
            Subscriptions.Add(Intercept<SystemDisposedEvent>(x => TrackedSystems.Remove(x.System.GetType())));
            ScriptEventRaised = new(this, nameof(ScriptEventRaised));
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
    }
}
