using System.Collections;
using System.Collections.Immutable;

namespace Fiero.Business
{
    public sealed class Pool<T>
    {
        public readonly int Capacity;
        public readonly GuaranteedWeightedItem<T>[] Choices;
        private readonly Queue<T> Queue = new();

        public Pool<T> WithCapacity(int capacity) => new(capacity, Choices);

        public Pool(int capacity, params GuaranteedWeightedItem<T>[] choices)
        {
            Capacity = capacity;
            Choices = choices;
        }

        public T Next()
        {
            if (Queue.Count == 0)
                Regen();
            return Queue.Dequeue();
        }

        private void Regen()
        {
            foreach (var item in Rng.Random.ChooseMultipleGuaranteedWeighted(Choices, Capacity))
                Queue.Enqueue(item);
        }
    }

    public sealed class PoolBuilder<T>
    {
        private readonly ImmutableDictionary<T, GuaranteedWeightedItem<T>> Items;
        private PoolBuilder(ImmutableDictionary<T, GuaranteedWeightedItem<T>> items) => Items = items;
        public PoolBuilder() : this(ImmutableDictionary.Create<T, GuaranteedWeightedItem<T>>()) { }

        public PoolBuilder<T> If(Func<bool> @if, Func<PoolBuilder<T>, PoolBuilder<T>> inner)
        {
            if (@if())
                return inner(this);
            return this;
        }
        public PoolBuilder<T> Guarantee(T item, float extraCopyWeight = 0, int minAmount = 1, int maxAmount = int.MaxValue)
        {
            if (minAmount <= 0) throw new ArgumentOutOfRangeException(nameof(minAmount));
            return new(Items.SetItem(item, new(item, extraCopyWeight, minAmount, maxAmount)));
        }
        public PoolBuilder<T> Include(T item, float weight, int maxAmount = int.MaxValue) => new(Items.SetItem(item, new(item, weight, 0, maxAmount)));

        public Pool<T> Build(int capacity) => new(capacity, Items.Values.ToArray());
    }
}
