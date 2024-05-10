namespace Fiero.Business
{
    public class Portal : Feature
    {
        [RequiredComponent]
        public PortalComponent PortalProperties { get; private set; }
    }
}
