namespace Fiero.Business.Utils;

/// <summary>
/// Like a debounce, but it allows a fixed number of hits per cooldown period before kicking in.
/// </summary>
public class DelayedDebounce : Debounce
{
    protected readonly List<DateTime> Hits = new();

    public readonly double MaxHits;

    public DelayedDebounce(TimeSpan cooldown, double maxHits) : base(cooldown)
    {
        MaxHits = maxHits;
    }

    protected override bool OnHit()
    {
        base.OnHit();
        Hits.RemoveAll(x => DateTime.UtcNow - x > Cooldown);
        Hits.Add(LastHit);
        return Hits.Count > MaxHits;
    }
}
