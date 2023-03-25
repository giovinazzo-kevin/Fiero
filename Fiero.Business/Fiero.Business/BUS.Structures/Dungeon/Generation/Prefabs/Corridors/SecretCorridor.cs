using Fiero.Core;
using Fiero.Core.Structures;

namespace Fiero.Business
{
    public class SecretCorridor : Corridor
    {
        public SecretCorridor(UnorderedPair<Coord> a, UnorderedPair<Coord> b, ColorName color) : base(a, b, color) { }
        protected override Chance DoorChance() => new(1, 1);
        protected override EntityBuilder<Feature> DoorFeature(GameEntityBuilders e, Coord c) => e.Feature_SecretDoor(Color);
    }
}
