using System;
using System.Linq;
using System.Reflection;

namespace Fiero.Core
{
    public readonly struct SystemMessage<TSystem, TArgs>
        where TSystem : EcsSystem
    {
        public readonly string Sender;
        public readonly TSystem System;
        public readonly TArgs Data;

        public SystemMessage(string sender, TSystem sys, TArgs msg)
        {
            Sender = sender;
            System = sys;
            Data = msg;
        }

        private string FormatData()
        {
            var data = Data;
            var flags = BindingFlags.Public | BindingFlags.Instance;
            var props = typeof(TArgs).GetProperties(flags)
                .Select(p => $"{p.Name} = {p.GetValue(data)}");
            var fields = typeof(TArgs).GetFields(flags)
                .Select(f => $"{f.Name} = {f.GetValue(data)}");
            return String.Join($", ", props.Concat(fields));

        }

        public override string ToString() => $"[{System.GetType().Name}] {Sender} ({FormatData()})";
    }
}
