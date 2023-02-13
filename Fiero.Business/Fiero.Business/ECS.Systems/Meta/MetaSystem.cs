using Fiero.Core;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unconcern.Common;

namespace Fiero.Business
{
    public partial class MetaSystem : EcsSystem
    {
        public record struct SystemEventField(FieldInfo System, FieldInfo Field, bool IsRequest);

        public readonly SystemRequest<MetaSystem, EventRaisedEvent, EventResult> EventRaised;

        public MetaSystem(EventBus bus) : base(bus)
        {
            EventRaised = new(this, nameof(EventRaised));
        }

        public static IEnumerable<SystemEventField> GetSystemEventFields()
        {
            foreach (var s in typeof(GameSystems).GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => f.FieldType.IsAssignableTo(typeof(EcsSystem))))
            {
                foreach (var f in s.FieldType.GetFields(BindingFlags.Public | BindingFlags.Instance))
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
