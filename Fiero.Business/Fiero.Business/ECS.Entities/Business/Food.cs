using Fiero.Core;

namespace Fiero.Business
{
    public class Food : Throwable
    {
        [RequiredComponent]
        public FoodComponent FoodProperties { get; private set; }
    }
}
