using Fiero.Core;

namespace Fiero.Business
{
    public class PortalComponent : EcsComponent
    {
        public FloorConnection Connection { get; set; }
        public bool Connects(FloorId a, FloorId b)
            => Connection.From == a && Connection.To == b
            || Connection.To == a && Connection.From == b;
    }
}
