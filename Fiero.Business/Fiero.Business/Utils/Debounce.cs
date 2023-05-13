using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fiero.Business.Utils;

public class Debounce
{
    public readonly TimeSpan Cooldown;
    protected DateTime LastHit;

    public bool IsOnCooldown => DateTime.UtcNow - LastHit < Cooldown;
    private SemaphoreSlim _semaphore = new(1);
    public bool IsDebouncing => _semaphore.CurrentCount == 0;

    public event Action<Debounce>? Fire;

    public Debounce(TimeSpan cooldown)
    {
        Cooldown = cooldown;
        Hit();
    }

    public virtual void Hit()
    {
        var shouldDebounce = OnHit();
        if (!shouldDebounce)
        {
            Fire?.Invoke(this);
        }
        else if (shouldDebounce && !IsDebouncing && IsOnCooldown)
        {
            _semaphore.Wait();
            _ = Task.Run(async () =>
            {
                while (IsOnCooldown)
                {
                    await Task.Delay(Cooldown / 10);
                }
                _semaphore.Release();
                Fire?.Invoke(this);
            });
        }
    }

    protected virtual bool OnHit()
    {
        LastHit = DateTime.UtcNow;
        return true;
    }
}
