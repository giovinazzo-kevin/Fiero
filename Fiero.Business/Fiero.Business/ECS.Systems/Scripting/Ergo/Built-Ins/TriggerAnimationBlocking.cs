using LightInject;

namespace Fiero.Business;

[SingletonDependency]
public sealed class TriggerAnimationBlocking : TriggerAnimationBase
{
    public TriggerAnimationBlocking(IServiceFactory services) : base(services, "play_animations_blocking")
    {
        IsBlocking = true;
    }
}
