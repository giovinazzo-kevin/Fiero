using LightInject;

namespace Fiero.Business;

[SingletonDependency]
public sealed class TriggerAnimationBlocking : TriggerAnimationBase
{
    public TriggerAnimationBlocking(IServiceFactory services) : base(services, "play_blocking")
    {
        IsBlocking = true;
    }
}
