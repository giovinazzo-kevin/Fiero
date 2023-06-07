namespace Fiero.Core.Extensions
{
    public record class WeightedItem<T>(T Item, float Weight);
    public record class GuaranteedWeightedItem<T>(T Item, float Weight, int MinAmount) : WeightedItem<T>(Item, Weight);
}
