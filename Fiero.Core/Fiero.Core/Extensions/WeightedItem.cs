namespace Fiero.Core.Extensions
{
    public record class WeightedItem<T>(T Item, float Weight, int MaxAmount = int.MaxValue);
    public record class GuaranteedWeightedItem<T>(T Item, float Weight, int MinAmount, int MaxAmount = int.MaxValue) : WeightedItem<T>(Item, Weight, MaxAmount);
}
