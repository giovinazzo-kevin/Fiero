using LightInject;

namespace Fiero.Business;

[SingletonDependency]
public sealed class TriggerAnimation : TriggerAnimationBase
{
    public TriggerAnimation(IServiceFactory services) : base(services, "play_animations")
    {
    }
}
