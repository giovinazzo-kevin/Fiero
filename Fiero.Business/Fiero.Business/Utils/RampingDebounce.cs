namespace Fiero.Business.Utils;

/// <summary>
/// A debounce that increases its debounce time as many requests hit, and slowly relaxes it later.
/// </summary>
public class RampingDebounce : Debounce
{
    public readonly TimeSpan MinCooldown;
    public readonly TimeSpan MaxCooldown;
    public readonly TimeSpan RampUpFactor;
    public readonly TimeSpan DecayFactor;

    public RampingDebounce(TimeSpan minCooldown, TimeSpan maxCooldown, TimeSpan rampUpFactor, TimeSpan decayFactor) : base(minCooldown)
    {
        MinCooldown = minCooldown;
        MaxCooldown = maxCooldown;
        RampUpFactor = rampUpFactor;
        DecayFactor = decayFactor;
    }

    protected override bool OnHit()
    {
        var now = DateTime.UtcNow;
        var elapsed = now - LastHit;
        LastHit = now;

        if (elapsed <= MinCooldown)
        {
            Cooldown += RampUpFactor;
        }
        else
        {
            Cooldown -= TimeSpan.FromSeconds(DecayFactor.TotalSeconds * (elapsed.TotalSeconds / MaxCooldown.TotalSeconds));
        }

        Cooldown = new TimeSpan(Math.Max(MinCooldown.Ticks, Math.Min(Cooldown.Ticks, MaxCooldown.Ticks)));

        return Cooldown > MinCooldown;
    }
}
