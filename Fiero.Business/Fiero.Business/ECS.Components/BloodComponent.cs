using Fiero.Core;
using System;

namespace Fiero.Business
{
    public class BloodComponent : EcsComponent
    {
        public int MaximumAmount { get; set; }
        public int Amount { get; private set; }
        public ColorName Color { get; set; }

        public bool TryAdd(int amount)
        {
            if(Amount + amount is { } sum && sum <= MaximumAmount) {
                Amount = sum;
                return true;
            }
            return false;
        }

        public bool TryRemove(int amount)
        {
            if (Amount - amount is { } sub && sub >= 0) {
                Amount = sub;
                return true;
            }
            return false;
        }
    }
}
