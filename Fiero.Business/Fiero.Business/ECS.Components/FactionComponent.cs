using Fiero.Core;

namespace Fiero.Business
{
    public class FactionComponent : EcsComponent
    {
        public FactionName Type { get; set; }
        public FactionRelationships Relationships { get; set; }
    }
}
