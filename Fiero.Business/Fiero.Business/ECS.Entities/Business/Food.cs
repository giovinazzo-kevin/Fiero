using Fiero.Core;

namespace Fiero.Business
{
    public class Food : Projectile
    {
        [RequiredComponent]
        public FoodComponent FoodProperties { get; private set; }
    }
}
